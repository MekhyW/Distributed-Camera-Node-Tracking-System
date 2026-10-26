# Configuration

## Software/Python/Config.py

### Nodes

SYSTEM_CONFIG is a list with one dictionary per node:

| Key | Example | Meaning |
|---|---|---|
| id | 0 | Node number (should match the firmware ID) |
| name | "Camera Node 0" | Label shown on the video feed |
| cam_url | "http://192.168.5.176:81/stream" | ESP32-CAM MJPEG URL |
| motor_ip | "192.168.5.183" | Motor ESP32 IP |
| motor_port | 3333 | Motor ESP32 UDP port |

### Tracking

| Setting | Default | Meaning |
|---|---|---|
| YOLO_MODEL_PATH | — | Path to your .pt weights |
| YOLO_SIZE | 320 | Inference image size. Higher is more accurate but slower. |
| CONFIDENCE | 0.4 | Minimum detection confidence |
| INFERENCE_INTERVAL | 0.050 s | Minimum time between YOLO runs per node |
| LOOKAHEAD_TIME | 0.4 s | How far ahead the motors aim |
| PREDICTION_TIMEOUT | 0.5 s | Stop tracking after the target has been missing this long |

### Display

| Setting | Default | Meaning |
|---|---|---|
| DISPLAY_WIDTH, DISPLAY_HEIGHT | 640, 480 | Size each frame is resized to |
| BUTTON_AREA_HEIGHT | 60 | Height of the strip under each feed |
| BTN_W, BTN_H | 140, 40 | CONNECT button size |

!!! warning
    The error calculation assumes a 640 × 480 frame (centre at 320, 240). If you change DISPLAY_WIDTH or DISPLAY_HEIGHT, update the constants in _udp_sender_loop in YOLOProcessing.py too.

## Firmware

See [Firmware → tuning constants](../getting-started/firmware.md#firmware-tuning-constants).

## Unity

See [Unity visualizer → inspector settings](../getting-started/unity-visualizer.md#useful-inspector-settings).

## Tuning tips

- Camera oscillates around the target: lower Kp or raise Kd in the firmware, or widen the dead-band.
- Camera lags behind a fast target: raise LOOKAHEAD_TIME a little, or raise Kp / MAX_RPM.
- Detections flicker: lower CONFIDENCE, or raise PREDICTION_TIMEOUT so the filter coasts longer.
- Host PC can't keep up: raise INFERENCE_INTERVAL or lower YOLO_SIZE.
