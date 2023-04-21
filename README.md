<p align="center">
  <a href="https://one-beyond.com">
    <img src="Logo.png" width="700" alt="One Beyond" />
  </a>
</p>

# One Beyond File Storage Providers Dependencies

```mermaid
 graph BT;
 B1[FileStorage.Infrastructure] --> A1[FileStorage.Domain];
 C1[FileStorage.FileSystem] --> B1[FileStorage.Infrastructure];
 D1[FileStorage.Azure] --> B1[FileStorage.Infrastructure];
```
