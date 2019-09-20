# sdoj
Programming language online judging, contest system.

Demo website: https://oj.starworks.cc
```
███████╗██████╗  ██████╗      ██╗
██╔════╝██╔══██╗██╔═══██╗     ██║
███████╗██║  ██║██║   ██║     ██║
╚════██║██║  ██║██║   ██║██   ██║
███████║██████╔╝╚██████╔╝╚█████╔╝
╚══════╝╚═════╝  ╚═════╝  ╚════╝ 
```

## Programming Language support
| Language     | Version                 |
| ------------ | ----------------------- |
| C#           | 7.3 on .NET 4.7.2       |
| Visual Basic | 16.0 on .NET 4.7.2      |
| C++          | MSVC C++ 19.20.27508.1  |
| C            | MSVC C 19.20.27508.1    |
| Python       | 3.7                     |
| Java         | 12                      |
| JavaScript   | Node.js v12.1.0-win-x64 |

## Technicial frameworks
* Website/backend: `ASP.NET MVC/Razor`
* Server push tool: `SignalR`
* Frontend framework: `knockoutjs/jQuery/bootstrap 3`
* Database: `SQL Server 2017/Entity Framework 6`

## Running structure
```
                              +---------+----------+
                              |   SQL Server 2017  |
                              +---------+----------+
                                        |
  +---------+----------+                |
  |      Fontend       |      +---------+----------+
  |  jQuery/knockout   |------|   ASP.NET MVC/EF   |
  |    bootstrap3      |      +---------+----------+
  +---------+----------+                |
                                        | (Communicate by SignalR/WebSocket)
                                        |
                              +---------+----------+
                              | sdoj judger agent  |
    +---------+-----------+   +---------+----------+      
    | Sandbox:            |             |        
    | Ensure all process  |<------------| (Compiler is runing on out of process)
    | run in safe-sandbox |             |
    +---------+-----------+            /|\ 
                    +-----------------/ | \-----------------+
                    |                   |                   |
          +---------+----------+        |         +---------+----------+
          |   .NET Compiler    |        |         |   C/C++ Compiler   |
          |  C#/Visual Basic   |        |         |  C#/Visual Basic   |
          +---------+----------+        |         +---------+----------+
                                       /|\                                 
                +---------------------/ | \---------------------+
                |                       |                       |
      +---------+----------+  +---------+----------+  +---------+----------+
      |  Node.js Compiler  |  |    Java Compiler   |  |   Python Compiler  |
      +---------+----------+  +---------+----------+  +---------+----------+
```


# Security notice
This repository is protected by our company's information security principle. 

This project should only transfered in our company. Without author or our company's permission, anyone/entity should never publish this code to public.

**DO NOT COMMIT ANY COMPANY/PERSONAL RELATED SENSITIVE INFORMATION TO THIS REPOSITORY**.

本仓库受公司信息安全条例保护。

本项目只允许在公司内部传播。未经作者和公司批准，禁止任何人将本代码公开到外网。

**禁止将任何公司/个人相关的敏感信息签入本项目**。

# How to use

## Step by step
* Open your deployed website, demo here: https://oj.starworks.cc
* Register a account on website, then a email with activation link should been sent to your mailbox;
* Click the link in the email to activate your account;
* Select a question on "所有题目" menu;
* Answer the question by submit your solution code;
* Your answer will be judged automatically online shortly.

## Programming language template

