# chat-console
A console application to chat with ChatGPT using the OpenAI Chat API. Requires a Paid account and an API key at OpenAI.


## Getting started
First you'll need [an OpenAI account](https://platform.openai.com/signup), second you'll need [a paid account](https://platform.openai.com/account/billing/payment-methods) by adding a payment method and lastly you'll need to create [an API key](https://platform.openai.com/account/api-keys) to use with this application.

When you have the API key start the application and run the command

    /set ApiKey <you-api-key>
  
  
The console will echo the new setting. For the `ApiKey`-setting you'll have to close the program (use `/quit`) and open it again. For all other settings the `/set` and `/reset` commands will be in effect immediately after being executed.


## Help
Run the command `/help` in the console to get help. The output is as follows:

 Command | Description |
| --- | --- |
| `/help` | Displays helpful information about the chatbot. |
| `/reset` | Clears any previous chat context. |
| `/export` | Exports the current chat history to a log. |
| `/clear` | Clears the chat window. |
| `/quit or /exit` | Ends the conversation with the chatbot. |

### The settings file
Settings are persisted in a json-file located under `%USERPROFILE%/.chat-console/settings.json`.

The `RequestParams` settings parameter corresponds to the chat completion request as [documented by OpenAI here.](https://platform.openai.com/docs/api-reference/chat/create#chat/create)

Other settings are local to the application.
| Setting | Description |
| --- | --- |
| `SystemMessage` | If set, this string will be used to set the "system"-message for instructing the model as to what and how it should act. Read more about it [here.](https://platform.openai.com/docs/guides/chat/introduction) |
| `ApiKey` | You API key. |

## Todo
- [x] Multiline input
- [x] Conversation logging
- [x] Count tokens for ContextLength instead of messages.
