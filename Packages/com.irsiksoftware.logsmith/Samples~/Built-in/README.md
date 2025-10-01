# LogSmith Built-in RP Demo

This sample demonstrates LogSmith visual debug rendering with Unity's Built-in Render Pipeline.

## Setup

1. Create a new scene
2. Add the `BuiltInRPDemo.cs` script to a GameObject in the scene
3. Add a Camera to the scene
4. Press Play

## What it demonstrates

- Debug overlay (always visible)
- Drawing simple shapes (lines, quads) using the visual debug API
- Automatic detection and activation of the Built-in RP adapter

## Features

The Built-in RP adapter works out of the box with no additional setup required.

## Troubleshooting

If shapes don't appear:
1. Verify the Camera component is present and enabled
2. Check that the `enableVisualDebug` setting is true in your LoggingSettings asset
3. Check the Console for any initialization warnings
