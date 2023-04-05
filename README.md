# chat-console
A console application to chat with ChatGPT using the OpenAI Chat API. Requires a Paid account and an API key at OpenAI.


## Getting started
First you'll need [an OpenAI account](https://platform.openai.com/signup), second you'll need [a paid account](https://platform.openai.com/account/billing/payment-methods) by adding a payment method and lastly you'll need to create [an API key](https://platform.openai.com/account/api-keys) to use with this application.

At first run a [settings.json](https://github.com/bdoner/chat-console/blob/master/README.md#the-settings-file) file is created. Set your `ApiKey` in that file and restart the application.

## Help
Run the command `/help` in the console to get help. The output is as follows:

 Command | Description |
| --- | --- |
| `/help` | displays helpful information about the chatbot. |
| `/reset` | clear chat context. Start from scratch. |
| `/export` | Exports the current chat history to a log. |
| `/prompts`| list prompts stored in the Prompts directory. |
| `/prompt <prompt file>`| select a prompt. Call with no promt-file to unset the prompt. |
| `/clear` | clears the chat window. |
| `/quit or /exit` | ends the conversation with the chatbot and close the bot. |

## The settings file
Settings are persisted in a json-file located under `%USERPROFILE%/.chat-console/settings.json`.

The `RequestParams` settings parameter corresponds to the chat completion request as [documented by OpenAI here.](https://platform.openai.com/docs/api-reference/chat/create#chat/create)

Other settings are local to the application.
| Setting | Description |
| --- | --- |
| `SystemMessage` | If set, this string will be used to set the "system"-message for instructing the model as to what and how it should act. Read more about it [here.](https://platform.openai.com/docs/guides/chat/introduction). Can be set for a session by using the `/prompt` command. |
| `ApiKey` | You API key. |

## Logs
Logs that have been generated with the `/export` command can be found under `%USERPROFILE%/.chat-console/Logs/`.

## Prompts
Prompts can be stored in the `%USERPROFILE%/.chat-console/Prompts/` directory.


## Todo
- [x] Multiline input
- [x] Conversation logging
- [x] Count tokens for ContextLength instead of messages.
