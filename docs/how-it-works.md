# How it works

## Detection and tracking (Python)

For every node, the tracker runs 3 things in parallel:

- A video thread (VideoStream) keeps the latest MJPEG frame and reconnects when the stream drops
- The main loop (DroneController.process) flips and resizes the frame to 640 × 480, advances the Kalman filter every frame, and runs YOLO at most every INFERENCE_INTERVAL seconds (default 50 ms, about 20 detections per second)
- A UDP thread sends the aim point to the motor controller at about 60 Hz

### Kalman filter

KalmanTracker is a 4-state constant-velocity filter on [x, y, vx, vy]

Learn a bit about the filter here:

<iframe 
  width="560" 
  height="315" 
  src="https://www.youtube-nocookie.com/embed/IFeCIbljreY?si=l_51t2-1fK2p1oUv" 
  title="YouTube video player" 
  frameborder="0" 
  allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" 
  allowfullscreen>
</iframe>

- Predict runs on every frame, so the estimate keeps moving smoothly between YOLO runs
- Correct runs when YOLO finds the target (the highest-confidence box is used)
- If nothing is detected for PREDICTION_TIMEOUT (0.5 s), tracking stops and the estimate returns to the image centre, which tells the motors to stop

The motors aim at a point LOOKAHEAD_TIME (0.4 s) ahead of the current estimate, which hides network and motor latency for a moving target

### Error signal

The aim point is converted to a normalised error in [-1, 1]:

$$
norm_x = \frac{x - 320}{320}
$$

$$
norm_y = \frac{240 - y}{240}
$$

and sent as plain text, e.g. 0.215,-0.083.

## Motor control (ESP32)

Every 10 ms the firmware runs a PD controller per axis:

$$
output = K_p \cdot error + K_d \cdot \frac{d(error)}{dt}
$$

- Errors inside the dead-band (0.1) count as zero, so the camera doesn't jitter around the centre.
- Output is clamped to the RPM limits and the stepper takes a few steps per tick.
- If no command arrives for 200 ms, the error is reset to zero (safety stop).

### Modes

| Mode | Code | Behaviour |
|---|---|---|
| Disabled | D | Default after boot. Ignores error commands. |
| Tracking | T | Follows the error signal. |
| Searching | S | Error has been (0, 0) for more than 500 ms, so the target is lost. Pans slowly until it's found again. |

### Orientation

The MPU6050 gives pitch from the accelerometer; the QMC5883L gives magnetic heading, corrected with calibration offsets and tilt-compensated. Both are smoothed with a low-pass filter (ALPHA = 0.15). Together with the GPS fix, they're sent to Unity 10 times per second.

## Triangulation (Unity)

NodeManager_Ascii converts each node's GPS coordinates to local metres around the origin you set, and turns pitch and heading into a direction. Each node therefore defines a ray in 3D: a start point and a direction.

MultiNodeTriangulation takes every node in tracking or searching mode and, when there are at least two, solves for the point p that minimises the sum of squared distances to all rays:

$$
\left(\sum_i \left(I - d_i d_i^\top\right)\right) p = \sum_i \left(I - d_i d_i^\top\right) o_i
$$

where *oᵢ* is the node position and *dᵢ* its unit direction. This is a 3 × 3 linear system, so it's cheap and works for any number of nodes. The result is kept only if it's within maxRange and inside every node's fieldOfViewCone, which rejects solutions from near-parallel rays.
