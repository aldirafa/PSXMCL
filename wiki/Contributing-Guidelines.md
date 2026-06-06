We welcome contributions and improvements to the PSXMCL library! If you have feature suggestions or would like to submit enhancement code, please follow these steps:

### How to Submit a Feature Suggestion or Code Contribution

1. **Fork the Repository**: Clone your fork locally.
2. **Create a Topic Branch**: Use a descriptive branch name:
   ```bash
   git checkout -b feature/your-awesome-feature
   ```
3. **Make Surgical Edits**:
   - Write clean, targeted F# code that preserves original behavior while adding value.
   - Update XML documentation (`///`) on any modified or added types and functions.
4. **Compile and Verify**:
   - Ensure the library builds successfully:
     ```bash
     dotnet build
     ```
   - Check that any existing or new unit tests pass:
     ```bash
     dotnet test
     ```
5. **Submit a Pull Request (PR)**:
   - Target the `main` branch of the upstream repository.
   - Outline what you added or changed in the PR body. Reference any related issues or discussions.
