# Hardware

The reference build uses two nodes. Everything below is per system, halve the counts for a single node (duh).

## Parts list

| Qty | Part | Why? |
|---|---|---|
| 2 | ESP32 development board | Motor controller, sensors, UDP |
| 2 | ESP32-CAM | Video stream |
| 4 | 28BYJ-48 stepper motor + ULN2003 driver | One pan and one tilt per node |
| 2 | MPU6050 IMU | Pitch |
| 2 | QMC5883L magnetometer | Heading |
| 2 | GPS module with NMEA output (e.g. NEO-6M) | Node position |
| 1 set per node | 3D-printed mount (see below) | Pan/tilt mechanism |
| — | 5V supply able to drive two steppers | Power |
| — | Wi-Fi network shared by all nodes and the host PC | Networking |

## 3D-printed mount

Print one set of these from Hardware/STL/ per node:

| File (prefix CameraNode_pan tilt camear_) | Part |
|---|---|
| lazy susan REAL base_1_Body1.stl | Fixed base |
| lazy susan base_1_Body1.stl | Rotating pan platform |
| lazy susan pin_1_Body1.stl | Pan pivot pin |
| Spur Gear (40 teeth)_1_Body1.stl | Driven gear |
| Spur Gear (20 teeth)_1_Body1.stl and Spur Gear (20 teeth) (1)_1_Body1.stl | Motor gears |
| Pivot Arm_1_Body1.stl, Pivot Arm 2_1_Body1.stl | Tilt arms |
| Camera Base_1_Body1.stl | Camera holder |
| camear pin_1_Body1.stl | Tilt pivot pin |

The editable source is Hardware/Cad/CameraNode.f3d (Fusion 360).

!!! note "Camera orientation"
    The Python tracker flips each frame vertically (cv2.flip(frame, 0)) because the camera sits upside down on the reference mount. If you mount yours the other way up, just remove that flip in YOLOProcessing.py.

## Wiring (motor-controller ESP32)

| Peripheral | Signal | ESP32 GPIO |
|---|---|---|
| Pan stepper (ULN2003) | IN1 / IN2 / IN3 / IN4 | 25 / 26 / 27 / 14 |
| Tilt stepper (ULN2003) | IN1 / IN2 / IN3 / IN4 | 13 / 18 / 19 / 5 |
| MPU6050 + QMC5883L (shared I²C) | SDA / SCL | 21 / 22 |
| GPS (UART2, 9600 baud) | ESP32 RX ← GPS TX | 16 |
| GPS | ESP32 TX → GPS RX | 17 |

Power the ULN2003 boards from the 5V supply, not from the ESP32's 3.3V pin (pretty please, it fries the poor thing lol), and share ground between everything.

!!! tip "Sensor orientation"
    The firmware remaps the IMU and magnetometer axes to match how they're mounted on the reference build (see the comments around the ACCELEROMETER MAPPING and MAGNETOMETER MAPPING blocks)

## ESP32-CAM

The camera boards run the standard AI-Thinker CameraWebServer example sketch, flashed separately from the motor firmware. It should expose the MJPEG stream at:

```
http://<CAM_IP>:81/stream
```

The tracker resizes frames to 640 × 480, so VGA resolution on the camera is a good match.
