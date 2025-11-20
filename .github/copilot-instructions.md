# AI Coding Rules for Small Corner Map

1. **Do not add code, types, or enum values that do not actually exist or are not supported by the current project.**
2. **Do not invent or suggest features, types, or fields unless they are present in the codebase or explicitly requested.**
3. **When writing or editing code, always write out the code in its entirety for the relevant section. Do not leave code half-written or incomplete.**
4. **If you are unsure if something exists, check the codebase before adding or suggesting it.**
5. **If a new type or feature is needed, confirm with the user before adding it.**
6. **Keep all code and documentation accurate and in sync with the actual project.**

---

# Additional Common AI Coding Rules for C# Projects

7. **Follow C# naming conventions:** Use PascalCase for types and methods, camelCase for local variables and parameters, and ALL_CAPS for constants.
8. **Prefer explicit access modifiers:** Always specify public, private, protected, or internal for types and members.
9. **Avoid magic numbers:** Use named constants or enums for all literal values with special meaning.
10. **Write clear, self-documenting code:** Use descriptive names and add XML or inline comments where necessary for clarity.
11. **Avoid code duplication:** Refactor repeated logic into methods or helper classes.
12. **Prefer composition over inheritance** where practical, to reduce tight coupling and increase flexibility.
13. **Handle exceptions and errors gracefully:** Use try-catch blocks where appropriate and avoid swallowing exceptions silently.
14. **Validate all inputs:** Check method parameters and user input for validity and null references.
15. **Keep methods short and focused:** Each method should do one thing and do it well.
16. **Write unit tests for critical logic** where possible, and ensure all tests pass before merging changes.
17. **Use async/await for asynchronous code** instead of manual threading, unless lower-level control is required.
18. **Dispose of unmanaged resources:** Implement IDisposable where needed and use 'using' statements for disposable objects.
19. **Avoid static state unless necessary:** Prefer instance members to reduce side effects and improve testability.
20. **Document public APIs:** Use XML documentation comments for all public classes, methods, and properties.
21. **Follow SOLID principles:** Ensure your code adheres to the principles of Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, and Dependency Inversion.
22. **
