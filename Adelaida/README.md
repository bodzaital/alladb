# Architecture

Program -> CommandApp<Executor> -> Executor -> Evaluator

Evaluator
- ctor(connection string)
- current collection name or "no collection"
- current document id or empty string
- list of evaluator command names
- history queue
- PushHistory(full user input)
- Evaluate(command, arguments)

Tasks
- Move history out of Evaluator and into Input
    - It should not be an evaluator method
    - if input == "history" show history, otherwise evaluate

- Split evaluators
    - Shared context in Evaluator (new context class)
    - Dynamically search and build evaluators
        - EvaluatorClass attribute on type
        - EvaluatorMethod, etc.
    - Evaluate method
        - Find instance with matching evaluator method
        - call evaluate on the instance with the context and args