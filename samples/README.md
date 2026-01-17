# ObsKit.NET Samples

This directory contains sample applications demonstrating ObsKit.NET usage.

## Prerequisites

Before running the samples, you must set up the OBS runtime in the sample's output directory.

### Quick Setup

1. Run the setup script from the repository root:

**Bash (Git Bash):**
```bash
./tools/setup-obs-runtime.sh 32.0.4 ./obs-runtime
```

2. Copy the runtime to each sample's output directory:

```powershell
# For Recording sample
Copy-Item -Recurse .\samples\obs-runtime\* .\samples\ObsKit.NET.Sample.Recording\bin\Debug\net9.0\

# For ReplayBuffer sample
Copy-Item -Recurse .\samples\obs-runtime\* .\samples\ObsKit.NET.Sample.ReplayBuffer\bin\Debug\net9.0\
```

3. Build and run:

```powershell
cd samples\ObsKit.NET.Sample.Recording
dotnet run
```

## Samples

### ObsKit.NET.Sample.Recording

Demonstrates basic video recording:
- Initialize OBS in headless mode
- Create a scene with monitor capture
- Record to MP4 file with x264/AAC encoding
- Display recording statistics

### ObsKit.NET.Sample.ReplayBuffer

Demonstrates replay buffer functionality:
- Initialize OBS in headless mode
- Create a scene with monitor capture
- Maintain a rolling 30-second buffer
- Save replays on demand

## Directory Structure After Setup

Each sample directory should look like this after setup:

```
ObsKit.NET.Sample.Recording/
├── bin/Debug/net9.0/
│   ├── ObsKit.NET.Sample.Recording.exe
│   ├── obs.dll
│   ├── obs-ffmpeg-mux.exe
│   ├── libobs-d3d11.dll
│   ├── ... (other OBS DLLs)
│   ├── data/
│   │   ├── libobs/
│   │   └── obs-plugins/
│   └── obs-plugins/
│       └── 64bit/
├── Program.cs
└── ObsKit.NET.Sample.Recording.csproj
```

## Troubleshooting

**"ERROR: OBS runtime not found!"**
- The OBS runtime has not been copied to the output directory
- Run the setup script and copy files as described above

**"Source ID 'monitor_capture' not found"**
- The `win-capture.dll` plugin is missing from `obs-plugins/64bit/`

**"Failed to start recording"**
- Ensure `obs-ffmpeg-mux.exe` is in the output directory
- Check that you have write permissions for the output path
