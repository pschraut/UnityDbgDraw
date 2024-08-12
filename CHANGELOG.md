# Changelog
All notable changes to this package are documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2024-08-12
### Fixed
 - Fixed ```ArgumentNullException``` during internal ```DbgDraw``` initialization due to missing a required shader. I've implemented a build pre-processor that automatically adds the required ```Hidden/DbgDraw-Shaded``` shader to the 'Always Included Shaders' list found under 'Project Settings > Graphics'. This fixes issue #6.
 - Fixed ```FindObjectOfType``` deprecated warning when using Unity 2023.1 and newer.
 
## [1.1.0] - 2022-01-05
### Fixed
 - Fixed ```DbgDraw.Plane``` not not being oriented along the plane normal.
 - Fixed ```DbgDraw``` not rendering anything when using Universal Render Pipeline (probably affected any scriptable render pipeline).

### Added
 - Added ```DbgDraw.Plane``` to "Test Everything" in Examples package.

### Changed
 - Bumped minimum required Unity version from 2018.3 to 2019.3.

## [1.0.0] - 2019-12-17
### Added
 - Public release
