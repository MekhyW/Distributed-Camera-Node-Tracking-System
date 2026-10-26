# Troubleshooting

## Motors don't move

- The node is disabled. Nodes boot disabled. Enable it from Unity or sendactive` over UDP (see [Network protocol](reference/network-protocol.md#enabling-a-node-without-unity)). The serial monitor printsMode set to ACTIVE`.
- Wrongmotor_ip` inConfig.py`. Check the IP the board prints on boot.
- Motor power. 28BYJ-48 motors need the 5 V supply through the ULN2003; the driver LEDs should flicker when moving.

## Camera moves the wrong way

- Tilt reversed: swap-outputY` /outputY` in thehandleMotor(tiltStepper, ...)` call.
- Pan reversed: negate the pan output in the same block, or swap two motor coils.
- Everything upside down: check thecv2.flip(frame, 0)` line inYOLOProcessing.py` against how your camera is mounted.

## Video feed says "Connecting..." or "No Signal"

- Openhttp://<CAM_IP>:81/stream` in a browser. If that fails, the problem is the ESP32-CAM or network, not the script.
- The ESP32-CAM serves one stream client at a time. Close any browser tab showing it.
- Click CONNECT under the feed to force a reconnect.

## Unity shows no nodes

- The firmware sends telemetry to a hard-coded IP (`192.168.5.184` by default). Set it to your Unity PC's IP and reflash.
- Allow inbound UDP 4444 in the PC's firewall.
- Check that Node Prefab is assigned onNodeManager_Ascii`; otherwise the component disables itself.
-Parse Failed!` warnings in the Console mean the telemetry format doesn't match; check that both sides use the format in [Network protocol](reference/network-protocol.md#motor-esp32-unity-port-4444).

## Nodes appear in the wrong place or point the wrong way

- No GPS fix: latitude and longitude are0`, so the node lands far from the origin. Wait for a fix outdoors.
- Origin far away: setoriginLat` /originLon` onNodeManager_Ascii` close to your nodes.
- Heading off: recalibrate the magnetometer (see [Firmware](getting-started/firmware.md#calibrating-the-magnetometer)) and keep it away from motors and steel.
- Pitch or yaw mirrored: toggleinvertPitch` /invertYaw`.

## No triangulation dot

- At least two nodes must be in tracking or searching mode.
- Rays that are nearly parallel, or a point beyondmaxRange` or outsidefieldOfViewCone`, are rejected. Spread the nodes farther apart.
