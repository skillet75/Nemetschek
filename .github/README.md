# Agentic Workflow Documentation

This repository uses an agentic AI development workflow designed to maximize reliability, minimize timeouts, and ensure high code quality through small, incremental steps.

## Folder Structure

- `.github/workflows_config/` : Contains all configuration for the agentic workflow.
    - `agents/`: Definitions of specialized agents (Planner, Implementer, etc.). Use these as system instructions or context when invoking an agent in Copilot Chat or Continue.
    - `prompts/`: Reusable prompt templates to guide specific tasks. Copy and paste content from here into your chat window for best results.
    - `instructions.md`: Global rules that apply to all agents working on this repository.

## The Workflow: Plan $\rightarrow$ Implement $\rightarrow$ Review

To ensure success, follow the recommended cycle for every feature or bug fix request:

1.  **PLAN**: Invoke a **Planner Agent**. Provide your high-level requirement and ask it to create an implementation plan broken into 5–15 minute tasks.
2.  **IMPLEMENT**: Once you have a plan, pick ONE task from the list and invoke an **Implementer Agent**, providing both the specific task description AND the full context of the original request/plan.
3.  **REVIEW**: After implementation is complete, provide the changes to a **Reviewer Agent** for verification against requirements and quality checks.

## How to Use with GitHub Copilot / Continue

### 1. Invoking an Agent
When starting a new task or responding to one:
- Copy the content of the relevant agent file (e.g., `.github/workflows_config/agents/planner.md`) into your chat window as part of your prompt.
24:- Example for planning: *"I want to add X feature. Act according to these instructions: [Paste planner.md contents]"*

### 2. Using Prompt Templates
For more complex or specific tasks, use the templates in `.github/workflows_config/prompts/:
1.  Open a template (e.g., `plan_feature.md`).
2.  Copy its content into your chat window.
3.  Fill in the bracketed information like `[Insert user request...]`.

## Adding New Agents
To add a new specialized agent:
1. Create a new `.md` file in `.agentic/agents/` (e.g., `security_auditor.md`).
2. Define its **Responsibilities**, **Workflow Integration** (how it fits into the cycle), and **Constraints**.

## Best Practices for Developers
- **Keep tasks small**: If a task looks like it will take more than 15 minutes, ask your Planner to break it down further.
- **Verify every step**: Don't move from Task 2 to Task 3 until you have reviewed the implementation of Task 2.
- **Don't skip documentation**: When adding new modules or complex logic, include a task in the plan for updating relevant docs/tests.