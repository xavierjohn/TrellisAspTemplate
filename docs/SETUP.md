# Repository Setup Guide

This guide explains how to configure your repository to use the FunctionalDDD Clean Architecture Agent workflows.

## Prerequisites

- GitHub repository (can be created from this template)
- GitHub Actions enabled
- .NET 10 SDK (for local development)

## Step 1: Enable GitHub Actions Permissions

To allow the scaffolding workflow to create pull requests automatically, you need to configure workflow permissions:

1. **Go to your repository settings**:
   ```
   https://github.com/YOUR_USERNAME/YOUR_REPO/settings/actions
   ```

2. **Scroll to "Workflow permissions"**

3. **Select**: ? **Read and write permissions**

4. **Check**: ? **Allow GitHub Actions to create and approve pull requests**

5. **Click "Save"**

## Step 2: Create Required Labels

The workflows use specific labels to trigger different actions:

### Create Labels via GitHub CLI

```powershell
# Scaffold label - triggers complete project scaffolding
gh label create "copilot-scaffold" \
  --description "Trigger complete project scaffolding" \
  --color "dc779c"

# Feature label - triggers feature addition
gh label create "copilot-feature" \
  --description "Add a new feature to the project" \
  --color "0e8a16"
```

### Create Labels via GitHub UI

1. Go to: `https://github.com/YOUR_USERNAME/YOUR_REPO/labels`
2. Click **"New label"**
3. Create two labels:

**Label 1: copilot-scaffold**
- Name: `copilot-scaffold`
- Description: `Trigger complete project scaffolding`
- Color: `#dc779c` (pink)

**Label 2: copilot-feature**
- Name: `copilot-feature`
- Description: `Add a new feature to the project`
- Color: `#0e8a16` (green)

## Step 3: Create Project Specification

Create a file at `.github/project-spec.yml` with your project specification:

```yaml
project:
  name: MyProject
  namespace: MyCompany.MyProject
  description: A brief description of your project

domain:
  aggregates:
    - name: MyAggregate
      id: MyAggregateId
      properties:
        - name: Name
          type: MyName
          required: true
      behaviors:
        - name: DoSomething
          description: Performs an action

  valueObjects:
    - name: MyAggregateId
      type: RequiredGuid
    - name: MyName
      type: RequiredString

application:
  queries:
    - name: GetMyAggregateById
      parameters:
        - id: MyAggregateId
      returns: MyAggregate

  commands:
    - name: CreateMyAggregate
      parameters:
        - name: MyName
      returns: MyAggregate

api:
  version: "2025-01-15"  # Use current date
  endpoints:
    - resource: myaggregates
      operations: [GET, POST, PUT, DELETE]
  observability:
    openTelemetry: true
    serviceName: MyProjectApi
```

See [examples/specs/](../examples/specs/) for more examples.

## Step 4: Test the Workflows

### Test Scaffolding

Create a GitHub issue to trigger scaffolding:

```powershell
gh issue create \
  --label "copilot-scaffold" \
  --title "Scaffold MyProject" \
  --body "Initial project scaffolding"
```

**What happens**:
1. Workflow triggers when the `copilot-scaffold` label is added
2. Reads `.github/project-spec.yml`
3. Creates a new branch: `scaffold-{issue-number}-{project-name}`
4. Generates project structure
5. Commits changes
6. Creates a pull request (if permissions are configured)

### Test Feature Addition

Create an issue to add a feature:

```powershell
gh issue create \
  --label "copilot-feature" \
  --title "Add Customer aggregate" \
  --body "Add Customer aggregate with email validation"
```

**What happens**:
1. Workflow triggers when the `copilot-feature` label is added
2. Reads the issue body
3. Creates a feature branch
4. Generates the requested feature code
5. Commits changes
6. Creates a pull request

## Step 5: Monitor Workflow Execution

### Via GitHub CLI

```powershell
# List recent workflow runs
gh run list --limit 5

# Watch a specific run
gh run watch <RUN_ID>

# View run details
gh run view <RUN_ID>
```

### Via GitHub UI

1. Go to the **Actions** tab: `https://github.com/YOUR_USERNAME/YOUR_REPO/actions`
2. Click on a workflow run to see details
3. Click on jobs to see step-by-step logs

## Troubleshooting

### Workflow Not Triggering

**Problem**: Issue created with label, but workflow doesn't run

**Solutions**:
- ? Verify the label name is exactly `copilot-scaffold` or `copilot-feature`
- ? Check that GitHub Actions is enabled in repository settings
- ? Ensure workflow files exist in `.github/workflows/`
- ? Wait 10-15 seconds for GitHub to process the label event

### PR Creation Fails

**Problem**: Workflow runs but fails at "Create PR" step

**Error**: `GitHub Actions is not permitted to create or approve pull requests`

**Solution**: Enable workflow permissions (see Step 1 above)

### YAML Parsing Errors

**Problem**: Workflow runs but fails to read project specification

**Solutions**:
- ? Validate YAML syntax: https://www.yamllint.com/
- ? Ensure `.github/project-spec.yml` exists
- ? Check indentation (use 2 spaces, not tabs)
- ? Verify required fields are present: `project.name`, `project.namespace`

### Missing Dependencies

**Problem**: Workflow fails with "yq not found" or similar errors

**Solution**: The workflow installs dependencies automatically. If it fails:
- ? Check the workflow logs for network issues
- ? Retry the workflow (re-add the label to trigger again)

## Advanced Configuration

### Customize Workflow Behavior

Edit `.github/workflows/copilot-scaffold.yml` or `.github/workflows/copilot-feature.yml`:

**Change .NET version**:
```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.0.x'  # Change to desired version
```

**Add custom steps**:
```yaml
- name: Custom Step
  run: |
    echo "Running custom logic"
    # Your custom commands here
```

### Use Different Branch Names

In the "Create Branch" step:

```yaml
- name: Create Branch
  run: |
    BRANCH="custom-prefix-${{ github.event.issue.number }}"
    git checkout -b "$BRANCH"
    echo "BRANCH=$BRANCH" >> $GITHUB_ENV
```

## Next Steps

1. ? **Create your first issue** with the `copilot-scaffold` label
2. ? **Review the generated PR**
3. ? **Merge when ready**
4. ? **Add features** using `copilot-feature` label

## Resources

- **[Project Specification Template](../docs/agent/project-spec-template.md)** - Complete YAML reference
- **[Iterative Demo Guide](../docs/agent/iterative-demo.md)** - Step-by-step tutorial
- **[Feature Template](.github/feature-template.md)** - How to request features
- **[Examples](../examples/specs/)** - Sample project specifications

## Support

- ?? [Documentation](https://github.com/xavierjohn/FunctionalDDD)
- ?? [Discussions](https://github.com/xavierjohn/FunctionalDddAspTemplate/discussions)
- ?? [Issues](https://github.com/xavierjohn/FunctionalDddAspTemplate/issues)

---

**Happy scaffolding!** ??

*Your first issue will scaffold a complete Clean Architecture project in ~2 minutes.*
