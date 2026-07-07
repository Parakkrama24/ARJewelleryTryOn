# AR Jewellery Try-On

A mobile AR application built with **Unity** that lets users virtually try on jewellery (rings, necklaces) in real time using **MediaPipe hand landmark detection** and **Unity AR Foundation**.

---

## 📱 Features

- **Virtual Ring Try-On** — Place 3D ring models on any finger using real-time hand landmark tracking
- **Finger Toggle** — Switch the ring between Ring, Middle, and Thumb fingers with a button
- **Hand-Relative Scaling** — Ring automatically grows/shrinks as the hand moves toward or away from the camera
- **Necklace Try-On** — AR-based necklace placement using Unity AR Foundation
- **Multiple Ring Models** — Cycle through different ring designs at runtime
- **Scene Navigation** — Home screen → Hand Tracking scenes → AR scenes → back
- **Landmark Dot Toggle** — Show or hide hand skeleton independently from the ring

---

## 🏗️ Technology Stack

| Technology | Purpose |
|-----------|---------|
| **Unity 6** | Game engine & rendering |
| **Universal Render Pipeline (URP) 17.2** | Rendering pipeline |
| **AR Foundation 6.3.5** | Cross-platform AR framework |
| **ARCore 6.3.1** | Android AR backend |
| **ARKit 6.3.1** | iOS AR backend |
| **MediaPipe Unity Plugin** (`com.github.homuler.mediapipe`) | Hand landmark detection |
| **XR Interaction Toolkit 3.3.0** | XR input & interaction |
| **Unity Input System 1.15** | Input handling |

---

## 🗂️ Project Structure

```
ARJewelleryTryOn/
│
├── Assets/
│   ├── _project/
│   │   ├── Scenes/
│   │   │   ├── Home.unity              # Main menu / navigation hub
│   │   │   ├── HandTracking.unity      # Ring try-on with MediaPipe
│   │   │   ├── RingTryOn.unity         # AR ring scene
│   │   │   └── Kneckless.unity         # AR necklace scene
│   │   │
│   │   └── _scripts/
│   │       ├── Ring/
│   │       │   ├── RingAnnotationAnchor.cs   # Core ring placement (annotation-based)
│   │       │   └── LandmarkFollower.cs        # Legacy screen-space follower
│   │       │
│   │       ├── HandTracking/
│   │       │   ├── HandTrackingManager.cs    # Singleton — holds MediaPipe results
│   │       │   ├── HandDebugger.cs           # Debug overlay for landmarks
│   │       │   └── ModelManager.cs           # Cycles between ring 3D models
│   │       │
│   │       └── UI/
│   │           └── HomeScreen.cs             # Scene navigation & camera reset
│   │
│   ├── Models/                  # 3D jewellery assets (.fbx / .obj)
│   │   ├── RING.fbx
│   │   ├── RING 2.fbx
│   │   ├── 7-stone-ring.fbx
│   │   ├── Necklace.fbx
│   │   └── ...
│   │
│   └── MediaPipeUnity/          # MediaPipe Unity Plugin scenes & scripts
│       └── Samples/Scenes/
│           └── Hand Landmark Detection/
│               ├── Hand Landmark Detection 1.unity
│               ├── Hand Landmark Detection 2.unity
│               ├── HandLandmarkerRunner.cs
│               └── HandLandmarkDetectionConfig.cs
│
└── Packages/
    ├── com.github.homuler.mediapipe   # MediaPipe hand tracking
    └── manifest.json
```

---

## 🧠 Architecture

### Ring Placement — Annotation Anchor Approach

The core ring placement system (`RingAnnotationAnchor.cs`) works by reading the **world-space position** of MediaPipe `PointAnnotation` GameObjects directly — rather than converting raw normalized landmark coordinates through screen space (which caused jitter on mobile).

```
MediaPipe detects hand
  → HandLandmarkerRunner receives result
  → HandTrackingManager.UpdateResult() stores it
  → MultiHandLandmarkListAnnotation positions PointAnnotation[n] in world space
  → RingAnnotationAnchor reads PointAnnotation[n].transform.position each frame
  → Ring moves to that world position (+ offset)
  → Ring scales relative to hand width (landmark 5 ↔ 17 distance)
  → Ring rotates to align with finger direction (landmark n → n+1)
```

### Why Not SetParent?

When `SetLandmarkRadius(0)` is called to hide landmark dots, `PointAnnotation.localScale` becomes `(0,0,0)`. A parented ring would inherit this scale and disappear. Instead, the ring **copies world position** each frame — making ring visibility completely independent of landmark dot visibility.

### Scene Navigation & Camera Reset

When switching between **MediaPipe scenes** (Hand Landmark Detection) and **AR scenes** (ARDemo), the AR session holds the camera exclusively. Without resetting, the camera fails to reopen in MediaPipe scenes.

`HomeScreen.cs` calls `WebCamSource.ResetPermission()` before loading any MediaPipe scene, and `arSession.Reset()` before leaving AR scenes, ensuring clean camera handoff.

---

## 📜 Key Scripts

### `RingAnnotationAnchor.cs`
Tracks ring to a finger landmark using annotation world positions.

| Inspector Field | Default | Description |
|----------------|---------|-------------|
| `Multi Hand Annotation` | — | Reference to `MultiHandLandmarkListAnnotation` |
| `Finger Landmarks` | `{14, 10, 3}` | Ring PIP, Middle PIP, Thumb IP |
| `Finger Names` | `{"Ring", "Middle", "Thumb"}` | Display names for UI |
| `Base Scale` | `(1,1,1)` | Ring size at reference hand distance |
| `Scale Multiplier` | `1.0` | Fine-tune overall ring size |
| `Reference Hand Width` | `0.05` | Calibrated hand width (world units) |
| `Track Rotation` | `true` | Align ring with finger direction |
| `Rotation Smooth Speed` | `10` | Rotation smoothing speed |
| `Scale Smooth Speed` | `8` | Scale smoothing speed |

**Public Methods:**
```csharp
void ToggleFinger()           // Cycles Ring → Middle → Thumb → Ring...
void SetFingerIndex(int i)    // Jump to specific finger slot
void SetLandmarkIndex(int n)  // Set by raw landmark number
string CurrentFingerName      // "Ring" / "Middle" / "Thumb"
```

---

### `HandTrackingManager.cs`
Singleton that stores the latest `HandLandmarkerResult` from MediaPipe and provides smoothed landmark positions via an **adaptive velocity-aware filter**.

- When hand is **still** → high smoothing (`0.65`) eliminates micro-jitter
- When hand **moves fast** → low smoothing (`0.15`) keeps ring responsive
- When hand **re-appears** after being lost → instant position snap

---

### `HandLandmarkDetectionConfig.cs`
MediaPipe model configuration. Mobile-specific values for better accuracy:

| Setting | Desktop | Mobile |
|---------|---------|--------|
| `ImageReadMode` | CPUAsync | **GPU** |
| `MinHandDetectionConfidence` | 0.5 | **0.7** |
| `MinHandPresenceConfidence` | 0.5 | **0.7** |
| `MinTrackingConfidence` | 0.5 | **0.7** |

---

### `HomeScreen.cs`
Handles scene transitions between MediaPipe and AR scenes.

```csharp
void onStartAR(int sceneIndex)  // Load any scene; resets camera if going to MediaPipe
void backtoHome()               // Return to Home scene (index 0) with camera reset
```

**Inspector Field:** `arSceneIndices` — set the build indices of your AR scenes here.

---

## 🚀 Getting Started

### Requirements
- Unity 6 (6000.x)
- Android Build Support or iOS Build Support
- Android device with ARCore support, or iOS device with ARKit support

### Build Steps

1. Clone the repository
2. Open in **Unity 6**
3. Open **Build Settings** → switch platform to **Android** or **iOS**
4. Add the following scenes to Build Settings (in order):
   - `Home` (index 0)
   - `Hand Landmark Detection 1` (index 1)
   - `Hand Landmark Detection 2` (index 2)
   - `RingTryOn` (index 3)
   - `Kneckless` (index 4)
5. On `HomeScreen` GameObject, set `arSceneIndices` to `[3, 4]` in the Inspector
6. Build and deploy to device

### Camera Permissions
Camera permission is requested automatically on first launch via MediaPipe's `WebCamSource`. On Android, ensure `CAMERA` permission is declared in the manifest (Unity adds this automatically for AR Foundation builds).

---

## 🔧 Tuning Guide

### Ring Size Calibration (`Reference Hand Width`)
1. Hold your hand at a comfortable distance from the camera
2. Temporarily add this to `RingAnnotationAnchor.Update()`:
   ```csharp
   Debug.Log(Vector3.Distance(scaleA.transform.position, scaleB.transform.position));
   ```
3. Read the logged value
4. Set `Reference Hand Width` to that value in the Inspector
5. Remove the debug log

### Reducing Jitter on Mobile
- Increase `Smoothing Still` on `HandTrackingManager` (default `0.65`, try up to `0.75`)
- Reduce `Velocity Threshold` (default `0.015`) for more gradual switching
- Increase `Rotation Smooth Speed` on `RingAnnotationAnchor` for smoother orientation

---

## 🔑 Landmark Reference

MediaPipe Hand Landmark indices used in this project:

```
Landmark  5 → Index MCP   (used for hand width measurement)
Landmark 17 → Pinky MCP   (used for hand width measurement)

Landmark  3 → Thumb IP    (ring anchor — Thumb)
Landmark  4 → Thumb tip   (direction for Thumb rotation)

Landmark 10 → Middle PIP  (ring anchor — Middle finger)
Landmark 11 → Middle DIP  (direction for Middle rotation)

Landmark 14 → Ring PIP    (ring anchor — Ring finger)
Landmark 15 → Ring DIP    (direction for Ring rotation)
```

---

## 👥 Team

*(Add team member names here)*

---

## 📄 License

MIT License
