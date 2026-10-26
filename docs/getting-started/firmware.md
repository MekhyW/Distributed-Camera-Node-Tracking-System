# Firmware

Each node's motor-controller ESP32 runs Firmware/ID0.cpp (node 0) or Firmware/ID1.cpp (node 1). The two files are identical EXCEPT for:

- the node ID in the telemetry string (ID:0 / ID:1), and
- the tilt motor direction (handleMotor(tiltStepper, -outputY) vs outputY), which compensates for how the motor is mounted

To add a third node, copy one of them, change the ID, and check the tilt direction

## Tools

I recommend using PlatformIO (https://platformio.org/) but the OG Arduino IDE works too as long as you have the ESP32 board support. Then install these libs:

- Adafruit MPU6050
- Adafruit Unified Sensor
- TinyGPSPlus (TinyGPS++.h)
- QMC5883LCompass
- Stepper (already bundled with Arduino)

A minimal platformio.ini:

ini
[env:esp32dev]
platform = espressif32
board = esp32dev
framework = arduino
monitor_speed = 115200
lib_deps =
    adafruit/Adafruit MPU6050
    adafruit/Adafruit Unified Sensor
    mikalhart/TinyGPSPlus
    mprograms/QMC5883LCompass


## Edit before flashing

Open the file and change these values for your setup:

| What | Where in the file | Why |
|---|---|---|
| Wi-Fi name and password | ssid, password near the top | To connect to your network |
| Unity host IP | udp.beginPacket("192.168.5.184", 4444) | Where telemetry is sent, you should set it to the IP of the PC running Unity |
| Magnetometer calibration | mx_off, my_off, mz_off, mx_scale, my_scale, mz_scale | Hard/soft-iron correction for YOUR sensor |

!!! warning "Don't commit real Wi-Fi passwords"
    If you do this Mekhy will personally come to your location and bitchslap you.

### Calibrating the magnetometer

The offsets and scales in the repo were measured on the original hardware and probably won't match yours. The easiest way to do this is:

1. Print raw x, y, z readings from the QMC5883L over serial
2. Slowly rotate the node through every orientation for about a minute
3. For each axis, offset = (max + min) / 2
4. For each axis, radius = (max - min) / 2; then scale = average_radius / radius

## Flash and check

1. Flash ID0.cpp to node 0's motor ESP32 and ID1.cpp to node 1's
2. Open the serial monitor at 115200 baud
3. You should see WiFi Connected and System Ready
4. Note each board's IP address (from the serial output or your router). You'll need it for the Python config and Unity

!!! info "Nodes boot disabled"
    On power-up a node is in Disabled mode (isActive = false) and ignores tracking commands. Enable it from the Unity visualizer, or by sending the UDP text active to port 3333 (see [Network protocol](../reference/network-protocol.md#enabling-a-node-without-unity)).

## Firmware tuning constants

![Tuning](../tuning.jpg)

| Constant | Default | Meaning |
|---|---|---|
| Kp | 15.0 | Proportional gain (≈ 15 RPM at full error) |
| Kd | 5.0 | Derivative gain |
| DEADBAND_X, DEADBAND_Y | 0.1 | Errors smaller than this are treated as zero |
| MAX_RPM / MIN_RPM | 17 / 5 | Stepper speed limits |
| STEPS_PER_LOOP | 2 | Steps taken per control tick |
| SEARCH_RPM | 10 | Pan speed while searching |
| SEARCH_DELAY | 500 ms | Time at zero error before search mode starts |
| ALPHA | 0.15 | Low-pass filter factor for pitch and heading |
| localPort | 3333 | UDP port the node listens on |
