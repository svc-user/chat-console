# chat-console
A console application to chat with ChatGPT using the OpenAI Chat API. Requires a Paid account and an API key at OpenAI.


## Getting started
First you'll need [an OpenAI account](https://platform.openai.com/signup), second you'll need [a paid account](https://platform.openai.com/account/billing/payment-methods) by adding a payment method and lastly you'll need to create [an API key](https://platform.openai.com/account/api-keys) to use with this application.

When you have the API key start the application and run the command

    /set ApiKey <you-api-key>
  
  
The console will echo the new setting. For the `ApiKey`-setting you'll have to close the program (use `/quit`) and open it again. For all other settings the `/set` and `/reset` commands will be in effect immediately after being executed.


## Help
Run the command `/help` in the console to get help. The output is as follows:

| Command | Description |
| --- | --- |
| `/get <filter>` | Show all settings matching filter. Filter is a case-insensitive string used to filter settings. The filter `*` shows all settings. |
| `/set <setting> <value>` | Sets a setting value. If successful the updated setting will be shown. The setting key is case-insensitive, but must match the full setting name. |
| `/reset <setting>` | Remove the value of a setting. The setting key is case-insensitive, but must match the full setting name. |
| `/clear` | Clear the chat buffer window. |
| `/clearcontext` | Clear the conversation context. Good for starting a new conversation without references to past messages. |
| `/help` | This message. |
| `/quit` or `/exit` | Close the console window. |

### The help file
Settings are persisted in a json-file located under `%USERPROFILE%/.chat-console/settings.json`.

The `RequestParams` settings parameter corresponds to the chat completion request as [documented by OpenAI here.](https://platform.openai.com/docs/api-reference/chat/create#chat/create)

Other settings are local to the application.
| Setting | Description |
| --- | --- |
| `ConsoleTitle` | The title of the console window |
| `SystemMessage` | If set, this string will be used to set the "system"-message for instructing the model as to what and how it should act. Read more about it [here.](https://platform.openai.com/docs/guides/chat/introduction) |
| `UserName` | The name of you, the users, shown in the console. |
| `BotName` | The name of the bot to be shown in the console. |
| `ApiKey` | You API key. |
| `PS1` | The prompt character shown after yours and the bots name. |
| `ContextLength` | How many messages back in time should be stored and send along messages as context for the message. If set too high you might get errors, if set too low the language model will not understand references to previous messages. |

## Todo
- [ ] Multiline input
- [ ] Conversation logging
- [ ] Count tokens for ContextLength instead of messages.