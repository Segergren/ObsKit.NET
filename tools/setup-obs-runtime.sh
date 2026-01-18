#!/bin/bash
#
# Downloads and sets up OBS Studio runtime for use with ObsKit.NET.
#
# Usage:
#   ./setup-obs-runtime.sh [version] [output-path]
#
# Examples:
#   ./setup-obs-runtime.sh
#   ./setup-obs-runtime.sh 31.0.0
#   ./setup-obs-runtime.sh 31.0.0 ./my-app/obs-runtime
#

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
BOLD='\033[1m'
NC='\033[0m' # No Color

# Default version
DEFAULT_VERSION="31.0.0"

# Files/folders to exclude (browser and frontend-related)
EXCLUDE_FILES_WINDOWS=(
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

EXCLUDE_FILES_LINUX=(
    "obs-browser.so"
    "frontend-tools.so"
    "obs-websocket.so"
    "libcef.so"
    "chrome-sandbox"
    "snapshot_blob.bin"
    "v8_context_snapshot.bin"
    "icudtl.dat"
    "chrome_100_percent.pak"
    "chrome_200_percent.pak"
    "resources.pak"
)

EXCLUDE_FOLDERS=(
    "locales"
    "cef"
)

print_header() {
    echo -e "\n${CYAN}${BOLD}$1${NC}"
}

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

show_menu() {
    echo ""
    echo -e "${BOLD}Select Platform:${NC}"
    echo ""
    echo "  1) Windows (x64)"
    echo "  2) Linux (x64)"
    echo "  3) macOS (Universal)"
    echo ""
    echo "  q) Quit"
    echo ""
}

should_exclude() {
    local name="$1"
    local platform="$2"

    if [[ "$platform" == "windows" ]]; then
        for exclude in "${EXCLUDE_FILES_WINDOWS[@]}"; do
            if [[ "$name" == "$exclude" ]]; then
                return 0
            fi
        done
    else
        for exclude in "${EXCLUDE_FILES_LINUX[@]}"; do
            if [[ "$name" == "$exclude" ]]; then
                return 0
            fi
        done
    fi

    for exclude in "${EXCLUDE_FOLDERS[@]}"; do
        if [[ "$name" == "$exclude" ]]; then
            return 0
        fi
    done
    return 1
}

setup_windows() {
    local version="$1"
    local output_path="$2"

    local filename="OBS-Studio-${version}-Windows-x64.zip"
    local url="https://github.com/obsproject/obs-studio/releases/download/${version}/${filename}"

    print_step "Downloading OBS Studio ${version} for Windows..."
    echo "URL: $url"

    if command -v curl &> /dev/null; then
        curl -L -o "$TEMP_ZIP" "$url" --progress-bar
    elif command -v wget &> /dev/null; then
        wget -O "$TEMP_ZIP" "$url" --show-progress
    else
        print_error "ERROR: Neither curl nor wget found."
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
        print_error "ERROR: unzip not found."
        exit 1
    fi

    # Find the root folder
    local obs_root="$TEMP_EXTRACT"
    for dir in "$TEMP_EXTRACT"/*; do
        if [[ -d "$dir/bin" ]]; then
            obs_root="$dir"
            break
        fi
    done

    if [[ ! -d "$obs_root/bin" ]]; then
        print_error "ERROR: Could not find OBS directory structure."
        exit 1
    fi

    print_success "Extracted."

    print_step "Setting up runtime directory structure..."
    mkdir -p "$output_path"

    # Copy bin/64bit contents to root
    local bin_path="$obs_root/bin/64bit"
    if [[ -d "$bin_path" ]]; then
        echo "  Copying bin/64bit/* to root..."
        for item in "$bin_path"/*; do
            local name=$(basename "$item")
            if should_exclude "$name" "windows"; then
                echo "    Skipping: $name"
            else
                cp -r "$item" "$output_path/"
            fi
        done
    fi

    # Copy data folder
    if [[ -d "$obs_root/data" ]]; then
        echo "  Copying data/..."
        cp -r "$obs_root/data" "$output_path/"
    fi

    # Copy obs-plugins folder
    if [[ -d "$obs_root/obs-plugins" ]]; then
        echo "  Copying obs-plugins/..."
        mkdir -p "$output_path/obs-plugins/64bit"

        local plugins_64="$obs_root/obs-plugins/64bit"
        if [[ -d "$plugins_64" ]]; then
            for item in "$plugins_64"/*; do
                local name=$(basename "$item")
                if should_exclude "$name" "windows"; then
                    echo "    Skipping: $name"
                else
                    cp -r "$item" "$output_path/obs-plugins/64bit/"
                fi
            done
        fi
    fi

    echo ""
    echo "Directory structure:"
    echo "  $output_path/"
    echo "  ├── obs.dll, obs-ffmpeg-mux.exe, etc."
    echo "  ├── data/"
    echo "  │   ├── libobs/"
    echo "  │   └── obs-plugins/"
    echo "  └── obs-plugins/"
    echo "      └── 64bit/"
}

setup_linux() {
    local version="$1"
    local output_path="$2"

    # Linux uses different package names depending on version
    # Try the common formats
    local filename="OBS-Studio-${version}-Ubuntu-x86_64.tar.xz"
    local url="https://github.com/obsproject/obs-studio/releases/download/${version}/${filename}"

    print_step "Downloading OBS Studio ${version} for Linux..."
    echo "URL: $url"

    if command -v curl &> /dev/null; then
        if ! curl -L -o "$TEMP_ZIP" "$url" --progress-bar -f; then
            # Try alternative filename
            filename="obs-studio-${version}-linux-x86_64.tar.xz"
            url="https://github.com/obsproject/obs-studio/releases/download/${version}/${filename}"
            print_warning "Trying alternative: $filename"
            curl -L -o "$TEMP_ZIP" "$url" --progress-bar
        fi
    elif command -v wget &> /dev/null; then
        wget -O "$TEMP_ZIP" "$url" --show-progress || {
            filename="obs-studio-${version}-linux-x86_64.tar.xz"
            url="https://github.com/obsproject/obs-studio/releases/download/${version}/${filename}"
            print_warning "Trying alternative: $filename"
            wget -O "$TEMP_ZIP" "$url" --show-progress
        }
    else
        print_error "ERROR: Neither curl nor wget found."
        exit 1
    fi

    if [[ ! -f "$TEMP_ZIP" ]]; then
        print_error "ERROR: Download failed."
        print_warning "Note: Linux builds may not be available for all versions."
        print_warning "Consider installing OBS via your package manager instead:"
        print_warning "  Ubuntu/Debian: sudo apt install obs-studio"
        print_warning "  Fedora: sudo dnf install obs-studio"
        print_warning "  Arch: sudo pacman -S obs-studio"
        exit 1
    fi

    print_success "Download complete."

    print_step "Extracting archive..."
    mkdir -p "$TEMP_EXTRACT"

    if command -v tar &> /dev/null; then
        tar -xf "$TEMP_ZIP" -C "$TEMP_EXTRACT"
    else
        print_error "ERROR: tar not found."
        exit 1
    fi

    # Find the root folder
    local obs_root="$TEMP_EXTRACT"
    for dir in "$TEMP_EXTRACT"/*; do
        if [[ -d "$dir" ]]; then
            obs_root="$dir"
            break
        fi
    done

    print_success "Extracted."

    print_step "Setting up runtime directory structure..."
    mkdir -p "$output_path"
    mkdir -p "$output_path/lib"
    mkdir -p "$output_path/obs-plugins"

    # Copy libraries
    if [[ -d "$obs_root/lib" ]]; then
        echo "  Copying libraries..."
        cp -r "$obs_root/lib"/* "$output_path/lib/" 2>/dev/null || true
    fi

    if [[ -d "$obs_root/usr/lib" ]]; then
        echo "  Copying usr/lib..."
        find "$obs_root/usr/lib" -name "*.so*" -exec cp {} "$output_path/lib/" \; 2>/dev/null || true
    fi

    # Copy plugins
    if [[ -d "$obs_root/lib/obs-plugins" ]]; then
        echo "  Copying plugins..."
        for item in "$obs_root/lib/obs-plugins"/*; do
            local name=$(basename "$item")
            if should_exclude "$name" "linux"; then
                echo "    Skipping: $name"
            else
                cp -r "$item" "$output_path/obs-plugins/"
            fi
        done
    fi

    # Copy data
    if [[ -d "$obs_root/share/obs" ]]; then
        echo "  Copying data..."
        cp -r "$obs_root/share/obs" "$output_path/data"
    fi

    echo ""
    echo "Directory structure:"
    echo "  $output_path/"
    echo "  ├── lib/"
    echo "  │   └── libobs.so.0, etc."
    echo "  ├── obs-plugins/"
    echo "  │   └── *.so"
    echo "  └── data/"
    echo "      └── libobs/, obs-plugins/"
    echo ""
    print_warning "Note: You may also need to install OBS dependencies via your package manager."
    echo "  Ubuntu/Debian: sudo apt install libobs0 libobs-dev"
}

setup_macos() {
    local version="$1"
    local output_path="$2"

    # macOS uses .dmg files
    local filename="OBS-Studio-${version}-macOS-Universal.dmg"
    local url="https://github.com/obsproject/obs-studio/releases/download/${version}/${filename}"

    print_step "Downloading OBS Studio ${version} for macOS..."
    echo "URL: $url"

    if command -v curl &> /dev/null; then
        if ! curl -L -o "$TEMP_ZIP" "$url" --progress-bar -f; then
            # Try alternative filename
            filename="obs-studio-${version}-macos-universal.dmg"
            url="https://github.com/obsproject/obs-studio/releases/download/${version}/${filename}"
            print_warning "Trying alternative: $filename"
            curl -L -o "$TEMP_ZIP" "$url" --progress-bar
        fi
    elif command -v wget &> /dev/null; then
        wget -O "$TEMP_ZIP" "$url" --show-progress
    else
        print_error "ERROR: Neither curl nor wget found."
        exit 1
    fi

    if [[ ! -f "$TEMP_ZIP" ]]; then
        print_error "ERROR: Download failed."
        exit 1
    fi

    print_success "Download complete."

    # Check if we're on macOS for mounting
    if [[ "$(uname)" == "Darwin" ]]; then
        print_step "Mounting DMG..."
        local mount_point=$(mktemp -d)
        hdiutil attach "$TEMP_ZIP" -mountpoint "$mount_point" -nobrowse -quiet

        print_step "Copying OBS.app..."
        mkdir -p "$output_path"
        cp -R "$mount_point/OBS.app" "$output_path/"

        print_step "Unmounting DMG..."
        hdiutil detach "$mount_point" -quiet

        echo ""
        echo "Directory structure:"
        echo "  $output_path/"
        echo "  └── OBS.app/"
        echo "      └── Contents/"
        echo "          ├── Frameworks/"
        echo "          │   └── libobs.0.dylib, etc."
        echo "          ├── PlugIns/"
        echo "          │   └── *.so"
        echo "          └── Resources/"
        echo ""
        echo "Library paths for ObsKit.NET:"
        echo "  - $output_path/OBS.app/Contents/Frameworks"
        echo "  - $output_path/OBS.app/Contents/PlugIns"
    else
        print_warning "Cannot extract .dmg file on non-macOS system."
        print_warning "The DMG file has been downloaded to: $TEMP_ZIP"
        print_warning ""
        print_warning "To extract on macOS:"
        print_warning "  1. Copy the .dmg file to your Mac"
        print_warning "  2. Double-click to mount"
        print_warning "  3. Copy OBS.app to your desired location"

        # Move the DMG to output instead
        mkdir -p "$output_path"
        mv "$TEMP_ZIP" "$output_path/$filename"
        echo ""
        print_success "DMG saved to: $output_path/$filename"
    fi
}

# Main script
echo -e "${CYAN}${BOLD}"
echo "╔═══════════════════════════════════════════╗"
echo "║     ObsKit.NET - OBS Runtime Setup        ║"
echo "╚═══════════════════════════════════════════╝"
echo -e "${NC}"

# Get version
VERSION="${1:-}"
OUTPUT_PATH="${2:-}"

if [[ -z "$VERSION" ]]; then
    echo -n "Enter OBS version [$DEFAULT_VERSION]: "
    read VERSION
    VERSION="${VERSION:-$DEFAULT_VERSION}"
fi

echo ""
echo -e "Version: ${BOLD}$VERSION${NC}"

# Show menu
show_menu

echo -n "Enter choice [1-3]: "
read choice

case $choice in
    1)
        PLATFORM="windows"
        PLATFORM_NAME="Windows x64"
        ;;
    2)
        PLATFORM="linux"
        PLATFORM_NAME="Linux x64"
        ;;
    3)
        PLATFORM="macos"
        PLATFORM_NAME="macOS Universal"
        ;;
    q|Q)
        echo "Cancelled."
        exit 0
        ;;
    *)
        print_error "Invalid choice."
        exit 1
        ;;
esac

echo -e "Platform: ${BOLD}$PLATFORM_NAME${NC}"

# Get output path
if [[ -z "$OUTPUT_PATH" ]]; then
    DEFAULT_OUTPUT="./obs-runtime-$PLATFORM"
    echo -n "Enter output path [$DEFAULT_OUTPUT]: "
    read OUTPUT_PATH
    OUTPUT_PATH="${OUTPUT_PATH:-$DEFAULT_OUTPUT}"
fi

# Resolve to absolute path
OUTPUT_PATH="$(cd "$(dirname "$OUTPUT_PATH")" 2>/dev/null && pwd)/$(basename "$OUTPUT_PATH")" 2>/dev/null || OUTPUT_PATH="$(pwd)/$(basename "$OUTPUT_PATH")"

echo -e "Output:  ${BOLD}$OUTPUT_PATH${NC}"

# Check if output exists
if [[ -d "$OUTPUT_PATH" ]]; then
    print_error "\nERROR: Output directory already exists: $OUTPUT_PATH"
    echo "Remove it first or choose a different path."
    exit 1
fi

# Create temp directory
TEMP_DIR=$(mktemp -d)
TEMP_ZIP="${TEMP_DIR}/obs-download"
TEMP_EXTRACT="${TEMP_DIR}/extract"

cleanup() {
    rm -rf "$TEMP_DIR"
}
trap cleanup EXIT

# Run platform-specific setup
case $PLATFORM in
    windows)
        setup_windows "$VERSION" "$OUTPUT_PATH"
        ;;
    linux)
        setup_linux "$VERSION" "$OUTPUT_PATH"
        ;;
    macos)
        setup_macos "$VERSION" "$OUTPUT_PATH"
        ;;
esac

# Calculate size
SIZE=$(du -sh "$OUTPUT_PATH" 2>/dev/null | cut -f1)

echo ""
echo -e "${CYAN}===============================${NC}"
print_success "OBS Runtime setup complete!"
echo "Location: $OUTPUT_PATH"
echo "Size: $SIZE"
echo ""
echo "To use with your application:"
echo "  Copy the contents of '$OUTPUT_PATH' to your app's output directory"
echo ""
