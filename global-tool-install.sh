#!/usr/bin/env bash
set -e

echo "Packing mrGitTags..."
dotnet pack -c Release -o nupkg

echo "Installing global tool..."
dotnet tool install --global mrGitTags --add-source ./nupkg

echo "Done! You can now run 'mrGitTags' from anywhere."
