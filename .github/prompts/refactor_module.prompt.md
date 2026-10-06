# Prompt Template: Module Refactoring

**Role**: Refactorer Agent
**Context**: 
- **Module/File to Refactor**: [Insert file path]
- **Goal of Refactoring**: (e.g., "Improve readability", "Reduce duplication in X function")

**Instructions**:
1. Analyze the target module and identify areas for improvement based on the goal.
2. Plan small, incremental refactors that preserve existing behavior.
3. Implement one focused change at a time to minimize risk.
4. Ensure all tests pass after each step (if applicable).
5. Provide a summary of what was improved and why it is better now.

**Constraint**: Do not add new features or fix bugs during this refactoring session unless they are discovered as side effects, in which case you must stop and report them.