# LogSmith - Screenshot & Video Production Guide

## Overview
This guide provides step-by-step instructions for creating professional screenshots and promotional video for LogSmith's Asset Store submission.

## Technical Requirements

### Screenshots
- **Resolution**: 1920x1080 (16:9 aspect ratio)
- **Format**: PNG or JPEG (PNG preferred for UI)
- **File Size**: Under 5MB per image
- **Quality**: High quality, no compression artifacts
- **Count**: 5-6 screenshots minimum, 10 maximum

### Video
- **Length**: 30-60 seconds
- **Resolution**: 1920x1080 (1080p) minimum, 4K optional
- **Format**: MP4 (H.264 codec)
- **Frame Rate**: 30 or 60 fps
- **File Size**: Under 100MB (Asset Store limit)
- **Audio**: Optional background music, no voiceover required

## Screenshot Capture Plan

### Screenshot 1: Category Manager
**Scene**: Use Category Manager Editor Window
**Focus**: Runtime category configuration UI

**Setup Steps:**
1. Open Unity Editor (6000.2 LTS)
2. Open Window → LogSmith → Settings
3. Navigate to Categories tab
4. Ensure categories list shows multiple categories with:
   - Various colors assigned
   - Different log levels (Debug, Info, Warn, Error)
   - Mix of system and custom categories

**Capture:**
- Set Unity Editor to 1920x1080 window
- Use Windows Snipping Tool or Snagit
- Capture the entire LogSmith Settings window with Categories tab visible
- Include Unity's top menu bar for context

**Post-Processing:**
- Crop to focus on Categories tab
- Add subtle drop shadow if needed
- Ensure text is crisp and readable

---

### Screenshot 2: In-Game Debug Overlay
**Scene**: Play mode with active logging
**Focus**: Debug overlay with filtering

**Setup Steps:**
1. Open Sample scene: `Samples~/Demo/DemoScene.unity`
2. Enter Play mode
3. Press F1 to toggle debug overlay
4. Generate logs from multiple categories
5. Demonstrate filtering by:
   - Selecting specific category
   - Changing log level filter
   - Using search text

**Capture:**
- Use Unity Game View at 1920x1080
- Show overlay with 10-15 visible log entries
- Include different colored categories
- Show filter controls at top

**Post-Processing:**
- Capture directly from Game View
- No additional editing needed unless UI is too small

---

### Screenshot 3: Template Editor
**Scene**: Unity Editor with Template tab
**Focus**: Message template customization with live preview

**Setup Steps:**
1. Open Window → LogSmith → Settings
2. Navigate to Templates tab
3. Select a template (e.g., Default or Custom)
4. Show template tokens:
   - `{Timestamp}`, `{Level}`, `{Category}`, `{Message}`
5. Display live preview pane below

**Capture:**
- Editor window at 1920x1080
- Show template editing area and preview simultaneously
- Ensure token syntax is visible
- Include example output in preview

**Post-Processing:**
- Highlight key areas if needed
- Ensure readability of code/tokens

---

### Screenshot 4: Sink Configuration
**Scene**: Unity Editor with Sinks tab
**Focus**: Console and File sink configuration

**Setup Steps:**
1. Open Window → LogSmith → Settings
2. Navigate to Sinks tab
3. Show enabled sinks:
   - Console Sink with settings
   - File Sink with path configuration
4. Display extensibility message about custom sinks

**Capture:**
- Editor window showing Sinks tab
- Both Console and File sinks visible
- Configuration options displayed

**Post-Processing:**
- Clean crop
- Ensure all text is legible

---

### Screenshot 5: VContainer Integration
**Scene**: Code editor (VS Code or Rider)
**Focus**: VContainer DI code example

**Setup Steps:**
1. Open sample file: `Samples~/VContainerIntegration/GameManager.cs`
2. Show constructor injection:
```csharp
public class GameManager : IStartable
{
    private readonly ILog _log;

    public GameManager(ILog log)
    {
        _log = log;
    }

    public void Start()
    {
        _log.Info("Game started");
    }
}
```
3. Use syntax highlighting
4. Set editor theme to light or professional dark

**Capture:**
- Code editor at 1920x1080
- Focus on relevant code snippet
- Include file path/name in editor

**Post-Processing:**
- Crop to code area
- Ensure syntax highlighting is visible
- Add file name annotation if not visible

---

### Screenshot 6: Sample Scenes Overview
**Scene**: Unity Project window showing Samples folder
**Focus**: Comprehensive sample collection

**Setup Steps:**
1. Import all samples via Package Manager
2. Navigate to Project window
3. Show `Assets/Samples/LogSmith/[version]/` folder expanded
4. Display all sample folders:
   - BasicUsage
   - VContainerIntegration
   - CustomTemplates
   - CustomSinks
   - Demo
   - Built-in
   - URP
   - HDRP

**Capture:**
- Unity Editor Project window
- Folder hierarchy clearly visible
- All sample folders expanded one level

**Post-Processing:**
- Crop to Project window
- Ensure folder names are readable

---

## Video Production Plan

### Video Script (30-60 seconds)

**Segment 1: Introduction (0-8s)**
- **Visual**: LogSmith logo/title card
- **Text Overlay**: "LogSmith - Professional Unity Logging"
- **Transition**: Fade to Unity Editor

**Segment 2: Quick Start (8-18s)**
- **Visual**: Split screen
  - Left: Code snippet appearing line by line
    ```csharp
    var log = LogSmith.GetLogger("Game");
    log.Info("Starting game...");
    ```
  - Right: Unity Console showing output
- **Text Overlay**: "Get Started in Seconds"
- **Transition**: Dissolve to next feature

**Segment 3: Category Manager (18-28s)**
- **Visual**: Screen recording of Category Manager
  - Show creating a new category
  - Changing log level
  - Assigning color
- **Text Overlay**: "Runtime Category Management"
- **Transition**: Smooth pan to next feature

**Segment 4: Debug Overlay (28-38s)**
- **Visual**: Play mode with debug overlay
  - Press F1 to toggle
  - Filter logs by category
  - Change log level filter
  - Use search box
- **Text Overlay**: "In-Game Debug Overlay"
- **Transition**: Quick fade

**Segment 5: Professional Features (38-48s)**
- **Visual**: Quick cuts showing:
  - Template Editor (2s)
  - VContainer code (2s)
  - Test coverage badge (2s)
  - Compatibility matrix (2s)
- **Text Overlays**:
  - "Customizable Templates"
  - "VContainer DI Support"
  - "100% Test Coverage"
  - "Unity 2022.3+ | All Platforms"

**Segment 6: Sample Scenes (48-55s)**
- **Visual**: Quick montage of sample scene thumbnails
- **Text Overlay**: "8 Complete Sample Scenes"
- **Transition**: Fade to closing

**Segment 7: Call to Action (55-60s)**
- **Visual**: LogSmith logo with key features
- **Text Overlays**:
  - "LogSmith"
  - "Professional Logging for Unity"
  - "Available on Unity Asset Store"
- **End card**: GitHub URL, documentation link

---

### Video Recording Checklist

**Pre-Production:**
- [ ] Close unnecessary applications
- [ ] Set Unity Editor theme (Dark or Light, consistent)
- [ ] Configure screen resolution to 1920x1080
- [ ] Prepare all scenes and assets
- [ ] Test all demonstrations work smoothly
- [ ] Prepare recording software (OBS Studio, Camtasia, or ScreenFlow)

**Recording Settings (OBS Studio):**
- [ ] Video bitrate: 8000-10000 Kbps
- [ ] Encoder: x264
- [ ] Output format: MP4
- [ ] FPS: 60 (smooth motion for UI)
- [ ] Audio: Optional music track (royalty-free)

**During Recording:**
- [ ] Record each segment separately
- [ ] Allow 2-3 second buffer at start/end of each clip
- [ ] Perform actions slowly and deliberately
- [ ] Ensure cursor is visible and follows action
- [ ] Minimize mouse hunting or hesitation

**Post-Production Tools:**
- Adobe Premiere Pro (professional)
- DaVinci Resolve (free, professional-grade)
- Camtasia (easy, good for screen recordings)
- iMovie (Mac, simple editing)

**Editing Checklist:**
- [ ] Import all recorded segments
- [ ] Trim unnecessary portions
- [ ] Add transitions between segments (1-2s max)
- [ ] Add text overlays with clear, readable font
  - Font: Sans-serif (Helvetica, Arial, Roboto)
  - Size: Large enough to read on mobile
  - Position: Lower third typically
  - Duration: On screen long enough to read twice
- [ ] Add background music (royalty-free)
  - Volume: -20dB to -25dB (subtle)
  - Avoid copyright issues (YouTube Audio Library, Epidemic Sound)
- [ ] Color correction for consistency
- [ ] Final render at 1920x1080, 30-60fps, H.264

**Quality Checks:**
- [ ] Audio levels consistent (no clipping)
- [ ] Text readable on mobile devices
- [ ] No Unity errors/warnings visible
- [ ] Smooth transitions
- [ ] Total length: 30-60 seconds
- [ ] File size under 100MB
- [ ] Test playback on different devices

---

## Recommended Tools

### Screenshot Capture
- **Windows**: Snipping Tool, Snagit, Greenshot
- **macOS**: Screenshot utility (Cmd+Shift+4), Snagit
- **Cross-platform**: ShareX (Windows), OBS Studio (any OS)

### Video Recording
- **OBS Studio** (Free, open-source): Best for screen recording
- **Camtasia**: Professional, easy to use, paid
- **ScreenFlow** (Mac): Excellent for tutorials, paid
- **Bandicam** (Windows): Lightweight, good performance

### Video Editing
- **DaVinci Resolve** (Free): Professional-grade color correction
- **Adobe Premiere Pro**: Industry standard (subscription)
- **Final Cut Pro** (Mac): Professional editing (one-time purchase)
- **Camtasia**: Recording + editing in one tool

### Audio
- **YouTube Audio Library**: Free royalty-free music
- **Epidemic Sound**: Professional library (subscription)
- **Artlist**: High-quality music (subscription)
- **Free Music Archive**: Creative Commons music

### Graphics
- **Adobe Photoshop**: Professional image editing
- **GIMP**: Free alternative to Photoshop
- **Figma**: UI/UX design, good for text overlays
- **Canva**: Easy graphics creation

---

## Asset Store Submission Format

### Screenshot Upload
1. Login to Unity Asset Store Publisher Portal
2. Navigate to Media section
3. Upload screenshots in order (they appear as numbered slides)
4. Add captions (under each image)
5. Set primary screenshot (first one shown)

### Video Upload
**Option 1: Direct Upload**
- Upload MP4 directly to Asset Store
- Max 100MB file size

**Option 2: YouTube Link**
- Upload to YouTube (unlisted or public)
- Provide link in Asset Store submission
- Recommended: More flexibility, no size limit

### Optimization Tips
- **Screenshots**: Use PNG for UI, JPEG for scenes (smaller file size)
- **Video**: Compress with Handbrake if needed (target 50-80MB)
- **Naming Convention**:
  - `logsmith_screenshot_01_category_manager.png`
  - `logsmith_promo_video_60s.mp4`

---

## Review & Approval Process

**Before Submission:**
1. Review all screenshots on different displays
2. Watch video on mobile device to check readability
3. Get feedback from team members
4. Spell-check all text overlays and captions
5. Verify brand consistency (colors, fonts, messaging)

**Quality Standards:**
- All text must be readable at 1920x1080
- No typos or grammatical errors
- Professional appearance (no debug menus, error dialogs)
- Consistent Unity Editor theme across all screenshots
- Video pacing allows viewers to read text comfortably

---

*This guide is part of LogSmith's Asset Store preparation (Issue #4)*
