# Distributed Camera Node Tracking System

https://github.com/user-attachments/assets/d666da68-4de4-413b-ad2b-b3b17028fe75

This project is a distributed, multi-node camera tracking system capable of detecting, following, and triangulating the 3D position of a moving target (e.g. a drone) in real time. Instead of relying on a single high-end sensor, it combines multiple low-cost hardware nodes to achieve capabilities that no single camera can provide — such as spatial triangulation and wide-area distributed coverage.

Each physical node consists of:
- An ESP32-CAM that streams live video over Wi-Fi.
- A separate ESP32 motor controller that physically pan/tilts the camera via two stepper motors, driven by a PD control loop.
- An MPU6050 IMU (accelerometer + gyroscope) and a QMC5883L magnetometer for orientation and heading sensing.
- A GPS module for geo-location of each node.

The host PC runs a Python script that:
1. Pulls the MJPEG video stream from each node.
2. Runs a custom-trained YOLOv8 model to detect the target in each frame.
3. Fuses YOLO detections with a Kalman filter for smooth, predictive tracking.
4. Sends normalised pan/tilt error signals back to each ESP32 motor controller via UDP at ~60 Hz.

A Unity application receives telemetry from all nodes (pitch, magnetic heading, GPS coordinates, tracking state) and performs linear least-squares triangulation to estimate the target's real-world 3D position, visualised on a geo-referenced grid in real time.

```
┌──────────────────────────────────────────────────────────────┐
│                         HOST PC                              │
│                                                              │
│  Python (YOLOProcessing.py)          Unity Visualizer        │
│  ┌─────────────────────────┐         ┌────────────────────┐  │
│  │  VideoStream (MJPEG)    │         │  NodeManager_Ascii  │  │
│  │  YOLO Inference (YOLOv8)│         │  MultiNodeTriang.   │  │
│  │  KalmanTracker          │──UDP──▶ │  CameraVectorViz    │  │
│  │  DroneController        │◀──UDP── │  GPS Grid Drawer    │  │
│  └─────────────────────────┘         └────────────────────┘  │
│           │  UDP (norm. x,y)                  ▲              │
│           ▼                           UDP telemetry          │
└──────────────────────────────────────────────────────────────┘
           │                                    │
           ▼                                    │
┌─────────────────────┐             ┌───────────────────────┐
│  Node 0 (ESP32)     │             │  Node 1 (ESP32)        │
│  - ESP32-CAM stream │             │  - ESP32-CAM stream    │
│  - Stepper Pan/Tilt │             │  - Stepper Pan/Tilt    │
│  - MPU6050 + QMC    │             │  - MPU6050 + QMC       │
│  - GPS module       │             │  - GPS module          │
└─────────────────────┘             └───────────────────────┘
```

## How It Works — Key Components

### Kalman Filter (`KalmanTracker`)
A 4-state Kalman filter `[x, y, vx, vy]` runs every frame. YOLO detections (when available) correct the state; between detections the filter uses physics to coast the target position, preventing jitter and allowing smooth motor commands even when inference is skipped.

### PD Motor Controller (ESP32 firmware)
Each ESP32 receives a normalised error `(norm_x, norm_y)` in `[-1, 1]` via UDP. A PD controller converts this into stepper motor RPM and step direction. A dead-band prevents micro-jitter at centre. When the error is `(0, 0)` for more than 500 ms, the node enters Search Mode and slowly pans until the target is reacquired.

### 3D Triangulation (Unity — `MultiNodeTriangulation.cs`)
Using the heading and pitch reported by each node, two or more view rays are defined in world space. A linear least-squares solver finds the point in 3D space that minimises the sum of squared distances to all rays simultaneously, giving a robust estimate of the target's real-world position.

## Repository Layout

| Path | Description |
|---|---|
| `Firmware/ID0.cpp` | ESP32 firmware for Node 0 (motor control, sensors, UDP) |
| `Firmware/ID1.cpp` | ESP32 firmware for Node 1 |
| `Software/Python/Config.py` | Central configuration (IPs, ports, YOLO path, tuning) |
| `Software/Python/YOLOProcessing.py` | Main Python tracking script |
| `Software/Unity/` | Unity C# scripts for visualisation and triangulation |
| `Hardware/Cad/` | Fusion 360 `.3mf` source files for the camera mount |
| `Hardware/STL/` | Ready-to-print STL files |
| `Media/Demo-Video/` | One-minute project demonstration video |
| `Media/Prototypes/` | Photos of hardware evolution and early prototypes |

## Prerequisites

### Hardware
- 2× ESP32 development boards (motor controllers, one per node)
- 2× ESP32-CAM modules
- 2× 28BYJ-48 stepper motors + ULN2003 driver boards (pan axis)
- 2× 28BYJ-48 stepper motors + ULN2003 driver boards (tilt axis)
- 2× MPU6050 IMU modules
- 2× QMC5883L magnetometer modules
- 2× GPS modules (NMEA, e.g. NEO-6M)
- 3D-printed camera mounts (see `Hardware/STL/`)
- Shared Wi-Fi network for all nodes and the host PC

### Software — Firmware
- [PlatformIO](https://platformio.org/) (recommended) or Arduino IDE
- Libraries (install via PlatformIO / Library Manager):
  - `Adafruit MPU6050`
  - `Adafruit Unified Sensor`
  - `TinyGPS++`
  - `QMC5883LCompass`
  - `Stepper` (built-in Arduino)

### Software — Python Host
- Python 3.9+
- Dependencies:
  ```bash
  pip install ultralytics opencv-python numpy
  ```
- A trained YOLOv8 model (`.pt` file). The default path is set in `Config.py`.

### Software — Unity Visualizer
- Unity 2022 LTS or newer (no extra packages required beyond the built-in ones)

## How to Run

### 1. Flash the ESP32 Firmware

1. Open `Firmware/ID0.cpp` in PlatformIO or Arduino IDE.
2. Update the Wi-Fi credentials (`ssid` / `password`) and any IP addresses to match your network.
3. Flash to Node 0's ESP32 motor-controller board.
4. Repeat with `Firmware/ID1.cpp` for Node 1.
5. Power each node. Confirm the serial monitor prints `WiFi Connected` and `System Ready`.

> Note: The ESP32-CAM boards run the standard AI-Thinker camera web-server firmware. Flash them separately so they expose the MJPEG stream at `http://<IP>:81/stream`.

### 2. Configure the Python Script

Open `Software/Python/Config.py` and update:

```python
SYSTEM_CONFIG = [
    {
        "id": 0,
        "cam_url":   "http://<NODE0_CAM_IP>:81/stream",
        "motor_ip":  "<NODE0_MOTOR_ESP32_IP>",
        "motor_port": 3333
    },
    {
        "id": 1,
        "cam_url":   "http://<NODE1_CAM_IP>:81/stream",
        "motor_ip":  "<NODE1_MOTOR_ESP32_IP>",
        "motor_port": 3333
    }
]

# Path to your trained YOLOv8 weights file
YOLO_MODEL_PATH = r"C:\path\to\your\best.pt"
```

Key tuning parameters (also in `Config.py`):

| Parameter | Default | Description |
|---|---|---|
| `CONFIDENCE` | `0.4` | YOLO detection confidence threshold |
| `LOOKAHEAD_TIME` | `0.4 s` | Kalman predictive lookahead |
| `PREDICTION_TIMEOUT` | `0.5 s` | Stops tracking when target is lost this long |
| `INFERENCE_INTERVAL` | `0.05 s` | Time between YOLO inference calls (~20 FPS) |

### 3. Run the Python Tracker

```bash
cd Software/Python
python YOLOProcessing.py
```

A window will open showing all camera feeds side-by-side. Once a target appears in frame, each node will begin tracking it automatically.

- Green dot — Current estimated position of the target.
- Red dot — Predicted future position (Kalman lookahead).
- Click the CONNECT button under any feed to force-reconnect a dropped stream.
- Press `q` to quit.

### 4. Open the Unity Visualizer (Optional)

1. Open the Unity project in `Software/Unity/` using Unity 2022+.
2. Ensure the Unity scene has a `NodeManager_Ascii` component configured to listen on UDP port `4444`.
3. Enter Play mode. The nodes will appear on a GPS-referenced grid, showing their orientation vectors, tracking state, and live triangulated target position.

---

Original project by — Alisher Khojayev  
Edit by — Mekhy W.
