# Render Pipeline Validation Testing

This document describes how to validate LogSmith's render pipeline (RP) adapters across supported Unity versions.

## Overview

LogSmith supports three render pipelines:
- **Built-in Render Pipeline** - Unity's traditional fixed-function pipeline
- **Universal Render Pipeline (URP)** - Optimized for mobile and lower-end platforms
- **High Definition Render Pipeline (HDRP)** - Advanced rendering for high-end platforms

Each pipeline has dedicated PlayMode tests to validate the visual debug overlay integration.

## Supported Unity Versions

LogSmith maintains compatibility across:
- **Unity 2022.3 LTS** (2022.3.50f1+)
- **Unity 2023 LTS** (2023.2.20f1+)
- **Unity 6000.2 LTS** (6000.0.26f1+, Unity 6)

## Test Coverage

### PlayMode Tests by Pipeline

| Test Suite | Pipeline | Test Count | Coverage |
|------------|----------|------------|----------|
| `BuiltInRenderPipelineAdapterTests` | Built-in | 8 | Initialization, camera rendering, cleanup |
| `URPAdapterTests` | URP | 8 | ScriptableRendererFeature, volume rendering |
| `HDRPAdapterTests` | HDRP | 8 | CustomPass, HDRP volume integration |

### Shared Tests

`RenderPipelineAdapterServiceTests` validates automatic pipeline detection and adapter selection across all three pipelines.

## Running RP Validation Tests

### Option 1: Automated Multi-Version Validation (Recommended)

Run the comprehensive validation script to test all pipelines across all Unity versions:

```powershell
.\run-rp-validation.ps1
```

This will:
1. Detect installed Unity versions (2022.3, 2023, 6000.2)
2. Run PlayMode tests for Built-in, URP, and HDRP on each version
3. Generate detailed results for each pipeline/version combination
4. Create a summary report in `TestOutputs/RP-Validation-Summary-*.txt`

**Custom validation:**

```powershell
# Test only URP on Unity 6
.\run-rp-validation.ps1 -UnityVersions @("6000.2.5f1") -Pipelines @("URP")

# Test Built-in and URP on 2022.3 and 2023
.\run-rp-validation.ps1 -UnityVersions @("2022.3.50f1", "2023.2.20f1") -Pipelines @("Built-in", "URP")
```

### Option 2: Single Version Testing

Use the standard test runner with Unity version parameter:

```powershell
.\run-tests.ps1 -UnityVersion 6000.2.5f1
```

This runs all tests (including RP tests) on the specified Unity version.

### Option 3: Manual Unity Test Runner

For interactive debugging:

1. Open the Unity Editor (any supported version)
2. Window → General → Test Runner
3. Select **PlayMode** tab
4. Filter by test suite:
   - `BuiltInRenderPipelineAdapterTests`
   - `URPAdapterTests`
   - `HDRPAdapterTests`
5. Click **Run Selected**

## CI/CD Integration

### GitHub Actions Workflow

The RP validation matrix is configured in `.github/workflows/rp-validation.yml`:

```yaml
strategy:
  matrix:
    unityVersion: [2022.3.50f1, 2023.2.20f1, 6000.0.26f1]
    pipeline: [Built-in, URP, HDRP]
```

This creates 9 test jobs (3 Unity versions × 3 pipelines) that run in parallel on self-hosted Windows runners.

**Status:** Currently disabled (`if: false`) pending Director CI infrastructure setup. See issue #26.

### Enabling CI

When ready to enable:

1. Ensure self-hosted runners have all Unity versions installed
2. Configure `UNITY_LICENSE` secret in repository settings
3. Remove `if: false` from `.github/workflows/rp-validation.yml`
4. Push to trigger validation

## Test Results

### Output Files

All test outputs are saved to `TestOutputs/`:

**Multi-version validation:**
- `RP-Validation-Summary-{timestamp}.txt` - Overall summary
- `RP-{version}-{pipeline}-results-{timestamp}.xml` - NUnit XML results
- `RP-{version}-{pipeline}-log-{timestamp}.txt` - Unity console log

**Standard test runner:**
- `TestResults-for-claude-{timestamp}.csv` - Claude-readable format
- `Test-Results-{timestamp}.html` - Human-readable HTML report

### Interpreting Results

**Success criteria:**
- All RP adapter tests must pass on all supported Unity versions
- 100% test pass rate required (no failures, no errors)
- Coverage preserved across versions

**Common failure modes:**
- Missing render pipeline package (URP/HDRP not installed)
- Unity version compatibility issues
- Platform-specific rendering bugs
- Pipeline API changes between Unity versions

## Package Dependencies

### Built-in Pipeline
No additional packages required - supported out of the box.

### URP
Requires `com.unity.render-pipelines.universal`:
- Unity 2022.3: URP 14.x
- Unity 2023: URP 15.x
- Unity 6: URP 17.x

### HDRP
Requires `com.unity.render-pipelines.high-definition`:
- Unity 2022.3: HDRP 14.x
- Unity 2023: HDRP 15.x
- Unity 6: HDRP 17.x

**Note:** Package versions are managed via Unity's Package Manager and follow Unity's version compatibility.

## Troubleshooting

### Unity Not Found
```
Unity X.X.X not found at: C:\Program Files\Unity\Hub\Editor\X.X.X\Editor\Unity.exe
```
**Solution:** Install the missing Unity version via Unity Hub.

### URP/HDRP Tests Fail
```
MissingReferenceException: The object of type 'UniversalAdditionalCameraData' has been destroyed
```
**Solution:** Ensure URP/HDRP packages are installed for the test project. Check `Packages/manifest.json`.

### All Tests Skipped
```
No test results file generated
```
**Solution:** Check Unity log file for compilation errors or test discovery issues.

## Acceptance Criteria (Issue #40)

- [x] RP tests exist for Built-in, URP, HDRP
- [x] Test runner script supports multiple Unity versions
- [x] CI workflow defined for 3×3 matrix (versions × pipelines)
- [ ] All tests pass on 2022.3, 2023, and 6000.2 LTS
- [ ] 100% test coverage preserved across versions
- [ ] CI enabled and green (blocked by issue #26)

## Related Documentation

- [CI Matrix Documentation](CI-Matrix.md)
- [Testing Strategy](Testing-Strategy.md)
- [Render Pipeline Architecture](../README.md#architecture)

## References

- GitHub Issue: [#40 RP.8 CI Matrix — RP Validation](https://github.com/DakotaIrsik/LogSmith/issues/40)
- Epic: [Render Pipeline Support](https://github.com/DakotaIrsik/LogSmith/milestone/8)
