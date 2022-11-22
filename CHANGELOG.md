# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.0.2] - 2022-08-31
### Added
- Added path checks to avoid processing of scripts found in "Packages/" or in the "Library/"
- Added check to prepend the UnityEngine namespace to the Tooltip attribute only if needed, to avoid redundant declaration
- Escaped double quotes as this could lead to errors with Tooltip string content

## [0.0.3] - 2022-11-18
### Added
- removed backslashes from converted tooltips

## [0.0.4] - 2022-11-22
### Added
- created more reliable method for finding & converting illegal backslashes
- double quotation marks are now converted to simple quotation marks

## [0.0.5] - 2022-11-22
### Added
- Plugins folder is now excluded from Tooltip generation
