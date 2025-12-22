# Repository Setup

## 🚨 MANDATORY FIRST STEP

**Before scaffolding ANY code, create .gitignore file!**

### Why This Matters
Without a proper `.gitignore`, Git will track:
- ❌ Compiled binaries (`bin/`, `obj/`)
- ❌ Build outputs (`*.dll`, `*.exe`, `*.pdb`)
- ❌ IDE files (`.vs/`, `*.user`)
- ❌ NuGet packages
- ❌ Test results

This bloats the repository and causes merge conflicts.

---

## 📋 How to Create .gitignore

**Use the official Visual Studio .gitignore template from GitHub:**

https://github.com/github/gitignore/blob/main/VisualStudio.gitignore

### Download Command
```bash
# Download official template
curl https://raw.githubusercontent.com/github/gitignore/main/VisualStudio.gitignore -o .gitignore
```

### Manual Method
1. Visit https://github.com/github/gitignore/blob/main/VisualStudio.gitignore
2. Copy the entire content
3. Create `.gitignore` in your repository root
4. Paste the content

---

## ✅ Verification Checklist

After creating .gitignore, verify:

- [ ] `.gitignore` exists in repository root
- [ ] File is not empty (should be ~400 lines from official template)
- [ ] File includes `bin/` and `obj/` patterns
- [ ] File includes `*.dll`, `*.exe`, `*.pdb` patterns
- [ ] File includes `.vs/` pattern
- [ ] File includes NuGet package patterns
- [ ] File includes test result patterns

---

## 🚨 Critical Note

**Always create .gitignore BEFORE:**
- Scaffolding projects
- Building the solution
- Committing any code

This prevents build artifacts from ever being tracked.

---

## 📸 Documentation

**After scaffolding is complete:**

1. **Run the API project**
   ```bash
   cd Api/src
   dotnet run
   ```

2. **Open Swagger UI**
   - Navigate to `http://localhost:5000/swagger` or `https://localhost:5001/swagger`

3. **Take a screenshot**
   - Capture the full Swagger UI showing all API endpoints
   - Save as `docs/swagger-ui.png` in the repository root
   - This provides visual documentation of the API structure

4. **Add to README**
   - Include the screenshot in the project README.md
   - Shows developers what endpoints are available at a glance

**Example README section:**
```markdown
## API Documentation

The API is documented using Swagger/OpenAPI. When running locally, access the interactive documentation at:
- HTTP: http://localhost:5000/swagger
- HTTPS: https://localhost:5001/swagger

![Swagger UI](docs/swagger-ui.png)
