# Network protocol

All traffic is plain-text UDP on the local network, except video, which is HTTP.

## Ports

| Port | Protocol | Listener | Purpose |
|---|---|---|---|
| 81 | HTTP | ESP32-CAM | MJPEG stream at /stream |
| 3333 | UDP | Motor ESP32 | Error commands and mode commands |
| 4444 | UDP | Unity (host PC) | Node telemetry |

Allow UDP 4444 inbound in the host PC's firewall, or Unity won't receive anything.

## Python → motor ESP32 (port 3333)

Sent about 60 times per second per node.

```
<norm_x>,<norm_y>
```

| Field | Range | Meaning |
|---|---|---|
| norm_x | -1 … 1 | Horizontal error; positive = target right of centre |
| norm_y | -1 … 1 | Vertical error; positive = target above centre |

0.000,0.000 means "no target". After 500 ms of this, an active node enters search mode.

## Unity → motor ESP32 (port 3333)

| Message | Effect |
|---|---|
| active | Enable the node (tracking / searching) |
| disabled | Disable the node and stop the motors |

Case-insensitive; surrounding whitespace is ignored.

### Enabling a node without Unity

You can enable a node from any machine on the network, for example with Python:

```python
import socket
socket.socket(socket.AF_INET, socket.SOCK_DGRAM).sendto(b"active", ("<MOTOR_IP>", 3333))
```

## Motor ESP32 → Unity (port 4444)

Sent 10 times per second per node, to the IP hard-coded in the firmware.

```
ID:0,M:T,P:12.3,MAG:254.7,LAT:34.146787,LON:-118.388581
```

| Field | Meaning |
|---|---|
| ID | Node ID |
| M | Mode: D disabled, T tracking, S searching |
| P | Pitch in degrees (filtered) |
| MAG | Magnetic heading in degrees, 0–360 (filtered) |
| LAT, LON | GPS position, 6 decimal places (0.000000 with no fix) |
