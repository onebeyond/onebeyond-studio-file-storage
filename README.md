<p>
  <a href="https://one-beyond.com">
    <img src="Logo.png" width="300" alt="One Beyond" />
  </a>
</p>

[![Nuget version](https://img.shields.io/nuget/v/OneBeyond.Studio.FileStorage.Domain?style=plastic)](https://www.nuget.org/packages/OneBeyond.Studio.FileStorage.Domain)
[![Nuget downloads](https://img.shields.io/nuget/dt/OneBeyond.Studio.FileStorage.Domain?style=plastic)](https://www.nuget.org/packages/OneBeyond.Studio.FileStorage.Domain)
[![License](https://img.shields.io/github/license/OneBeyond/onebeyond-studio-file-storage?style=plastic)](LICENSE)
[![Maintainability](https://api.codeclimate.com/v1/badges/84e810be2f54c7f1a34d/maintainability)](https://codeclimate.com/github/onebeyond/onebeyond-studio-file-storage/maintainability)
[![Test Coverage](https://api.codeclimate.com/v1/badges/84e810be2f54c7f1a34d/test_coverage)](https://codeclimate.com/github/onebeyond/onebeyond-studio-file-storage/test_coverage)

# Introduction
On Beyond Studio File Storage is a set of .NET libraries that helps you to abstract file storage in your application.
At this moment, we support two types of storage:
- [File System Storage](https://www.nuget.org/packages/OneBeyond.Studio.FileStorage.FileSystem)
- [Azure Blobs Storage](https://www.nuget.org/packages/OneBeyond.Studio.FileStorage.Azure)

### Supported .NET version:

7.0

### Installation

The library that contains IFileStorage abstraction:

`dotnet new install OneBeyond.Studio.FileStorage.Domain`

The library that contains IFileStorage implementation based on File System:

`dotnet new install OneBeyond.Studio.FileStorage.FileSystem`

The library that contains IFileStorage implementation based on Azure Blobs:

`dotnet new install OneBeyond.Studio.FileStorage.Azure`

### Documentation

For more detailed documentation, please refer to our [Wiki](https://github.com/onebeyond/onebeyond-studio-file-storage/wiki)

### Contributing

If you want to contribute, we are currently accepting PRs and/or proposals/discussions in the issue tracker.
