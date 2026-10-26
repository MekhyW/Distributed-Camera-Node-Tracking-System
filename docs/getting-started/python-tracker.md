# Python tracker

My favorite part yay!
Software/Python/YOLOProcessing.py reads every camera stream, detects the target with YOLOv8, smooths it with a Kalman filter and steers each node's motors.

## Install

Python 3.9 or newer:

```bash
pip install ultralytics opencv-python numpy
```

For real-time performance on more than one node, install a CUDA-enabled PyTorch build if you have an NVIDIA GPU; ultralytics picks it up automatically.
You also need a trained YOLOv8 model (.pt) for your target. The one I included tracks people so you can obliterate victims automatically lol. But you can train your own with Ultralytics on images of the object you want to track.

## Configure

Edit Software/Python/Config.py. Add one entry per node:

```python
SYSTEM_CONFIG = [
    {
        "id": 0,
        "name": "Camera Node 0",
        "cam_url": "http://<NODE0_CAM_IP>:81/stream",
        "motor_ip": "<NODE0_MOTOR_ESP32_IP>",
        "motor_port": 3333
    },
    {
        "id": 1,
        "name": "Camera Node 1",
        "cam_url": "http://<NODE1_CAM_IP>:81/stream",
        "motor_ip": "<NODE1_MOTOR_ESP32_IP>",
        "motor_port": 3333
    }
]

YOLO_MODEL_PATH = r"C:\path\to\your\best.pt"
```

All other settings are described in [Configuration](../reference/configuration.md).

## Run

```bash
cd Software/Python
python YOLOProcessing.py
```

A window should open showing every camera feed side by side.

| On screen | Meaning |
|---|---|
| Green dot | Current estimated target position |
| Red dot | Predicted position LOOKAHEAD_TIME seconds ahead (this is what the motors aim at) |
| "Connecting..." / "No Signal" | Stream not available yet; it retries automatically |
| CONNECT button | Click to force a reconnect of that feed |

Press q in the window to quit.

!!! note
    The tracker sends commands to the motors continuously, but a node only moves once it has been switched to active (from Unity or with a UDP active message). See [Network protocol](../reference/network-protocol.md).

## Adding more nodes

Add another dictionary to SYSTEM_CONFIG. The window grows by one 640-pixel column per node. Remember to flash a matching firmware with a new ID.
