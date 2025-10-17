# LogSmith HDRP Demo

This sample demonstrates LogSmith visual debug rendering with High Definition Render Pipeline (HDRP).

## Prerequisites

- HDRP package installed (`com.unity.render-pipelines.high-definition`)
- HDRP asset configured in Graphics Settings

## Setup

1. Install HDRP from Package Manager if not already installed
2. Create a new scene with HDRP template
3. Add a Custom Pass Volume to the scene
4. Add "LogSmith Overlay Custom Pass" to the Custom Pass Volume
5. Add the `HDRPDemo.cs` script to a GameObject in the scene
6. Press Play

## What it demonstrates

- Debug overlay (always visible)
- Drawing simple shapes using the HDRP adapter
- Automatic detection of HDRP and activation of the adapter

## Troubleshooting

If shapes don't appear:
1. Verify the Custom Pass Volume is in the scene and active
2. Check that the LogSmith Overlay Custom Pass is added
3. Ensure HDRP package is installed
4. Check Console for adapter initialization messages
