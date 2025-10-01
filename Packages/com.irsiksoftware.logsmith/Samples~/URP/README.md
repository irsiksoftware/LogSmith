# LogSmith URP Demo

This sample demonstrates LogSmith visual debug rendering with Universal Render Pipeline (URP).

## Prerequisites

- URP package installed (`com.unity.render-pipelines.universal`)
- URP Renderer asset configured in Graphics Settings

## Setup

1. Install URP from Package Manager if not already installed
2. Create a new scene with URP template
3. Open your URP Renderer asset
4. Add "LogSmith Overlay Renderer Feature" to the renderer features list
5. Add the `URPDemo.cs` script to a GameObject in the scene
6. Press Play

## What it demonstrates

- Debug overlay (always visible)
- Drawing simple shapes using the URP adapter
- Automatic detection of URP and activation of the adapter

## Troubleshooting

If shapes don't appear:
1. Verify the Renderer Feature is added to your URP Renderer asset
2. Check that visual debug is enabled in LoggingSettings
3. Make sure the URP package is installed
4. Check Console for adapter initialization messages
