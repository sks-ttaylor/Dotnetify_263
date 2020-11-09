Dotnetify Issue #263

Steps to Reproduce:

Clone this project: https://github.com/sks-ttaylor/Dotnetify_263.git

Open Project in VS 2019

run the project with IISExpress

using 2 browsers navigate to https://localhost:44396/chat/InstA in each

send a message from each browser

from one of the browsers, click the browser refresh button, wait for it to reload

this should cause the xception "AddUser: _disposableClass.IsDisposed" to be thrown

from either browser, send a message

this should cause the xception "SendMessage: _disposableClass.IsDisposed" to be thrown

