# Unity visualizer

The Unity side is optional for tracking, but it's where you can actually see triangulation and where you remotely enable/disable nodes, it's super cool.

The repo contains the C# scripts in Software/Unity/, not a full Unity project, so you'll create a scene and attach them. Don't worry though that usually only takes a few minutes.

## Requirements

- Unity 2022 LTS or newer (it's free!)
- Input System package (used by CameraRigController.cs and HoverTooltipUI.cs). In *Project Settings → Player → Active Input Handling*, choose *Input System Package* or *Both*.
- TextMeshPro (used by HoverTooltipUI.cs). Import the TMP Essential Resources when Unity prompts.

## Scene setup

1. Create a new 3D project and copy all .cs files from Software/Unity/ into Assets/Scripts/
2. Node prefab: Create an empty GameObject, add NodeController, NodeData and CameraVectorVisualizer, and save it as a prefab. (The manager adds NodeController and NodeData itself if they're missing.)
3. Manager: Create a GameObject called NodeManager, add NodeManager_Ascii and:
    - set Port to 4444 (must match the firmware),
    - assign the node prefab to Node Prefab,
    - set Origin Lat / Origin Lon to a point near your nodes. This GPS point becomes Unity's world origin
4. Triangulation: Add MultiNodeTriangulation to any GameObject. Optionally assign a Dot Material for the target marker
5. Ground grid: Add GpsGridDrawer and assign a line material. By default it draws a 4 km grid with 100 m spacing
6. Tooltip and control: Add HoverTooltipUI, assign the scene camera to Target Camera, and fill System Config with each node's id, motor_ip and motor_port (same values as in Config.py)
7. Fly camera: Add CameraRigController (class FlyMover) to the main camera

## Use it!

Press Play. Each node appears on the grid as soon as its first telemetry packet arrives (the Console logs Spawned new node: Node_0)

| Control | Action |
|---|---|
| Hold right mouse + move | Look around |
| W / A / S / D | Move |
| E / Q | Up / down |
| Left Shift | Move faster |
| Esc | Release the cursor |
| Hover a node, click the button | Toggle that node between active and disabled |

When at least two nodes are tracking (or searching) and their view rays agree, a dot marks the triangulated target position.

## Useful inspector settings

| Component | Field | Default | Notes |
|---|---|---|---|
| NodeManager_Ascii | invertPitch | true | Flip if nodes point down when they should point up |
| NodeManager_Ascii | invertYaw | false | Flip if heading turns the wrong way |
| MultiNodeTriangulation | maxRange | 500 m | Ignore solutions farther than this |
| MultiNodeTriangulation | fieldOfViewCone | 30° | A solution must fall inside each node's cone |
| CameraVectorVisualizer | rayLength | 200 | Length of the drawn view ray |
