# --- CONFIGURATION ---
SYSTEM_CONFIG = [
    {
        "id": 0,
        "name": "Camera Node 0",
        "cam_url": "http://192.168.5.176:81/stream",
        "motor_ip": "192.168.5.183",
        "motor_port": 3333
    },
    {
        "id": 1,
        "name": "Camera Node 1",
        "cam_url": "http://192.168.5.187:81/stream", # Change to your 2nd ESP32-CAM IP
        "motor_ip": "192.168.5.172",               # Change to your 2nd Motor ESP32 IP
        "motor_port": 3333
    }
]

# --- TUNING SETTINGS ---
YOLO_MODEL_PATH = r"C:\Users\Alienware R7\Documents\PlatformIO\Projects\camESP32\runs\detect\train6\weights\best.pt"
YOLO_SIZE = 320
CONFIDENCE = 0.4
LOOKAHEAD_TIME = 0.4
PREDICTION_TIMEOUT = 0.5 # Stop tracking if object lost for 500ms

# DISPLAY SETTINGS
DISPLAY_WIDTH = 640
DISPLAY_HEIGHT = 480
# UI Area at the bottom for the button
BUTTON_AREA_HEIGHT = 60 
BTN_W = 140
BTN_H = 40
BTN_X = (DISPLAY_WIDTH - BTN_W) // 2
BTN_Y = (BUTTON_AREA_HEIGHT - BTN_H) // 2

# PERFORMANCE SETTINGS
# 0.050 = Run YOLO every 50ms (approx 20 FPS for detection)
# Faster than before, but still throttled to prevent UI freezes.
INFERENCE_INTERVAL = 0.050