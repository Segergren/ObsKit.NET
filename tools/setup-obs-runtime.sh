#!/bin/bash
#
# Downloads and sets up OBS Studio runtime for use with ObsKit.NET.
#
# Usage:
#   ./setup-obs-runtime.sh <version> [output-path]
#
# Examples:
#   ./setup-obs-runtime.sh 32.0.4
#   ./setup-obs-runtime.sh 32.0.4 ./my-app/obs-runtime
#

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Files/folders to exclude (browser and frontend-related)
EXCLUDE_FILES=(
    "obs-browser.dll"
    "obs-browser.pdb"
    "obs-browser-page.exe"
    "obs-browser-page.pdb"
    "frontend-tools.dll"
    "frontend-tools.pdb"
    "obs-websocket.dll"
    "obs-websocket.pdb"
    "libcef.dll"
    "chrome_elf.dll"
    "libEGL.dll"
    "libGLESv2.dll"
    "snapshot_blob.bin"
    "v8_context_snapshot.bin"
    "vk_swiftshader.dll"
    "vk_swiftshader_icd.json"
    "vulkan-1.dll"
    "icudtl.dat"
    "chrome_100_percent.pak"
    "chrome_200_percent.pak"
    "resources.pak"
)

EXCLUDE_FOLDERS=(
    "locales"
)

print_step() {
    echo -e "\n${CYAN}>> $1${NC}"
}

print_success() {
    echo -e "${GREEN}$1${NC}"
}

print_warning() {
    echo -e "${YELLOW}$1${NC}"
}

print_error() {
    echo -e "${RED}$1${NC}"
}

should_exclude() {
    local name="$1"
    for exclude in "${EXCLUDE_FILES[@]}"; do
        if [[ "$name" == "$exclude" ]]; then
            return 0
        fi
    done
    for exclude in "${EXCLUDE_FOLDERS[@]}"; do
        if [[ "$name" == "$exclude" ]]; then
            return 0
        fi
    done
    return 1
}

# Check arguments
if [[ $# -lt 1 ]]; then
    echo "Usage: $0 <version> [output-path]"
    echo ""
    echo "Examples:"
    echo "  $0 32.0.4"
    echo "  $0 32.0.4 ./obs-runtime"
    exit 1
fi

VERSION="$1"
OUTPUT_PATH="${2:-./obs-runtime}"

# Resolve to absolute path
OUTPUT_PATH="$(cd "$(dirname "$OUTPUT_PATH")" 2>/dev/null && pwd)/$(basename "$OUTPUT_PATH")" || OUTPUT_PATH="$(pwd)/$2"

echo -e "${CYAN}ObsKit.NET - OBS Runtime Setup${NC}"
echo "==============================="
echo "Version: $VERSION"
echo "Output:  $OUTPUT_PATH"

# Check if output exists
if [[ -d "$OUTPUT_PATH" ]]; then
    print_error "\nERROR: Output directory already exists: $OUTPUT_PATH"
    echo "Remove it first or choose a different path."
    exit 1
fi

# Detect OS for download
FILENAME="OBS-Studio-${VERSION}-Windows-x64.zip"
URL="https://github.com/obsproject/obs-studio/releases/download/${VERSION}/${FILENAME}"

# Create temp directory
TEMP_DIR=$(mktemp -d)
TEMP_ZIP="${TEMP_DIR}/${FILENAME}"
TEMP_EXTRACT="${TEMP_DIR}/extract"

cleanup() {
    rm -rf "$TEMP_DIR"
}
trap cleanup EXIT

print_step "Downloading OBS Studio ${VERSION}..."
echo "URL: $URL"

if command -v curl &> /dev/null; then
    curl -L -o "$TEMP_ZIP" "$URL" --progress-bar
elif command -v wget &> /dev/null; then
    wget -O "$TEMP_ZIP" "$URL" --show-progress
else
    print_error "ERROR: Neither curl nor wget found. Please install one of them."
    exit 1
fi

if [[ ! -f "$TEMP_ZIP" ]]; then
    print_error "ERROR: Download failed."
    exit 1
fi

print_success "Download complete."

print_step "Extracting archive..."

mkdir -p "$TEMP_EXTRACT"

if command -v unzip &> /dev/null; then
    unzip -q "$TEMP_ZIP" -d "$TEMP_EXTRACT"
else
    print_error "ERROR: unzip not found. Please install it."
    exit 1
fi

# Find the root folder
OBS_ROOT=$(find "$TEMP_EXTRACT" -maxdepth 1 -type d | tail -1)
if [[ -d "$TEMP_EXTRACT/bin" ]]; then
    OBS_ROOT="$TEMP_EXTRACT"
else
    # Look for extracted folder
    for dir in "$TEMP_EXTRACT"/*; do
        if [[ -d "$dir/bin" ]]; then
            OBS_ROOT="$dir"
            break
        fi
    done
fi

if [[ ! -d "$OBS_ROOT/bin" ]]; then
    print_error "ERROR: Could not find OBS directory structure."
    exit 1
fi

print_success "Extracted to: $OBS_ROOT"

print_step "Setting up runtime directory structure..."

mkdir -p "$OUTPUT_PATH"

# 1. Copy bin/64bit contents to root
BIN_PATH="$OBS_ROOT/bin/64bit"
if [[ -d "$BIN_PATH" ]]; then
    echo "  Copying bin/64bit/* to root..."
    for item in "$BIN_PATH"/*; do
        name=$(basename "$item")
        if should_exclude "$name"; then
            echo "    Skipping (excluded): $name"
        else
            cp -r "$item" "$OUTPUT_PATH/"
        fi
    done
fi

# 2. Copy data folder
DATA_PATH="$OBS_ROOT/data"
if [[ -d "$DATA_PATH" ]]; then
    echo "  Copying data/..."
    cp -r "$DATA_PATH" "$OUTPUT_PATH/"
fi

# 3. Copy obs-plugins folder (excluding browser-related)
PLUGINS_PATH="$OBS_ROOT/obs-plugins"
if [[ -d "$PLUGINS_PATH" ]]; then
    echo "  Copying obs-plugins/..."
    mkdir -p "$OUTPUT_PATH/obs-plugins/64bit"

    PLUGINS_64="$PLUGINS_PATH/64bit"
    if [[ -d "$PLUGINS_64" ]]; then
        for item in "$PLUGINS_64"/*; do
            name=$(basename "$item")
            if should_exclude "$name"; then
                echo "    Skipping (excluded): $name"
            else
                cp -r "$item" "$OUTPUT_PATH/obs-plugins/64bit/"
            fi
        done
    fi
fi

print_step "Cleaning up..."
# Cleanup is handled by trap

print_success "Cleanup complete."

# Calculate size
SIZE=$(du -sh "$OUTPUT_PATH" | cut -f1)

echo ""
echo -e "${CYAN}===============================${NC}"
print_success "OBS Runtime setup complete!"
echo "Location: $OUTPUT_PATH"
echo "Size: $SIZE"
echo ""
echo "Directory structure:"
echo "  $OUTPUT_PATH/"
echo "  ├── obs.dll, obs-ffmpeg-mux.exe, etc."
echo "  ├── data/"
echo "  │   ├── libobs/"
echo "  │   └── obs-plugins/"
echo "  └── obs-plugins/"
echo "      └── 64bit/"
echo ""
echo "To use with your application:"
echo "  Copy the contents of '$OUTPUT_PATH' to your app's output directory"
