# sdoj
Programming language online judging, contest system.

Demo website: https://oj.starworks.cc
```
███████╗██████╗  ██████╗      ██╗
██╔════╝██╔══██╗██╔═══██╗     ██║
███████╗██║  ██║██║   ██║     ██║
╚════██║██║  ██║██║   ██║██   ██║
███████║██████╔╝╚██████╔╝╚█████╔╝
╚══════╝╚═════╝  ╚═════╝  ╚════╝  (by Flysha.Zhou in Shareworks)
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
* Server push tool: `SignalR/WebSocket/Long Pulling`
* Frontend framework: `knockoutjs/jQuery/bootstrap 3`
* Database/DAL: `SQL Server 2017/Entity Framework 6`

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
              +-----------------------/ | \-----------------------+
              |                         |                         |
    +---------+----------+    +---------+----------+    +---------+----------+
    |  Node.js Compiler  |    |    Java Compiler   |    |   Python Compiler  |
    +---------+----------+    +---------+----------+    +---------+----------+
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
* C#
  ```csharp
  using System;

  class Program
  {
      static void Main()
      {
          // 输入示例：Console.ReadLine(); // 返回了一行字符串，可通过String.Split()分隔为字符串数组
          // 输出示例：Console.WriteLine("Hello World");
          Console.WriteLine("Hello World");
      }
  }
  ```

* Python 3
  ```python
  """
    输入示例：
        s = input() # 读一行
    输出示例：
        print('Hello World') # 输出Hello World
  """
  print('Hello World');
  ```

* Java
  ```java
  public class Program {
      public static void main(String[] args) {
          // 输入示例：
          //     java.util.Scanner scanner = new java.util.Scanner(System.in);
          //     String str = scanner.nextLine(); // 读一行
          //     int n = scanner.nextInt();       // 读int
          // 输出示例：
          //     System.out.println("Hello World");
          System.out.println("Hello World");
      }
  }
  ```

* JavaScript/Node.js
  ```js
  'use strict';
  
  const readline = require("readline");
  const rl = readline.createInterface({
      input: process.stdin,
      output: process.stdout,
      terminal: false
  });
  
  function readlineAsync() {
      return new Promise(r => {
          rl.on("line", onData);
  
          function onData(s) {
              r(s);
              rl.off("line", onData);
          }
      });
  }
  
  main().then(() => rl.close());
  
  // 请在此处写代码
  async function main() {
      // 示例输入：const input = await readlineAsync();
      // 示例输出：console.log(`Hey ${input}!`);
  }
  ```

* Visual Basic
  ```vb
  Imports System

  Module Program
      Sub Main()
          ' 输入示例：Console.ReadLine() ' 返回了一行字符串，可能需要手动分隔
          ' 输出示例：Console.WriteLine("Hello World");
          Console.WriteLine("Hello World")
      End Sub
  End Module
  ```

* C++
  ```cpp
  #include <iostream>

  using namespace std;
  
  int main()
  {
      // 输入示例：cin >> a >> b;
      // 输出示例：cout << a << b;
      cout << "Hello World" << endl;
  }
  ```

* C
  ```c
  #include <stdio.h>
  
  int main()
  {
      // 输入示例：scanf("%d%d", &a, &b);
      // 输出示例：printf("%s", "Hello World");
      printf("Hello World");
  }
  ```

# Known other OJs
| Platform | Type        | Website                              | Juding           |
| -------- | ----------- | ------------------------------------ | ---------------- |
| **sdoj** | -           | https://code.shareworks.cn/sdcb/sdoj | **Server Push**  |
| Hustoj   | Open Source | https://github.com/zhblue/hustoj     | DB Short Pulling |
| Leetcode | Commercial  | https://leetcode.com                 | Queue            |
| POJ      | Commercial  | https://poj.org                      | Short pulling    |
| 51nod    | Commercial  | https://www.51nod.com/               | Short pulling    |