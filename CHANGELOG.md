# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

-

### Fixed

-

### Changed

-

### Removed

-

## [0.1.1] - 2026-06-27

### Added

- ReadOnly property attribute. And make all previous readonly field use it.

### Fixed

- Hide `tag` field from `UnityEngine.Component` in `Taggable` to avoid accidentally use it.
- Serious mistake that cause build error since incorrectly using the editor content.

### Changed

- Make generic install fully abstract.

### Removed

-

## [0.1.0] - 2026-06-06

### Added

- Unity package essential files
- Singleton implementation in both normal way and MonoBehaviour way
- Event system implementation (ref: https://github.com/m-gebhard/uni-utils/) with lock and snapshot-in-publish
  extension.
- Event queue implementation
- Reflection usage of getting type in runtime
- Serializable dictionary
- Object dumper to string
- String extension functions
- Generic registry storage method
- Attribute for injection and injection way to set ref
- Extend tag system
- Game bootstrapper, installer and service locator for un-singleton accessiable services
- Wrapped debug log utilities
- Compile flag toggler in editor
- QuadTree data structure implementation

