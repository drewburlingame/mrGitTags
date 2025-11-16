#!/usr/bin/env bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
WRAPPER_PATH="$HOME/.local/bin/mrGitTags"

# Build the project
echo "Building mrGitTags..."
dotnet build -c Release

# Create ~/.local/bin if it doesn't exist
mkdir -p "$HOME/.local/bin"

# Create wrapper script
echo "Creating wrapper at $WRAPPER_PATH..."
cat > "$WRAPPER_PATH" << EOF
#!/usr/bin/env bash
exec dotnet "$SCRIPT_DIR/bin/Release/net9.0/mrGitTags.dll" "\$@"
EOF

chmod +x "$WRAPPER_PATH"

# Check if ~/.local/bin is in PATH
if [[ ":$PATH:" != *":$HOME/.local/bin:"* ]]; then
    echo ""
    echo "WARNING: $HOME/.local/bin is not in your PATH"
    echo "Add this to your ~/.zshrc:"
    echo "  export PATH=\"\$HOME/.local/bin:\$PATH\""
else
    echo "Done! You can now run 'mrGitTags' from anywhere."
    echo "Just run 'dotnet build' to update after making changes."
fi
