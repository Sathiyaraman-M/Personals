# Personals

**Personals** is a personal utilities application designed to help you manage your day-to-day tasks with ease. 

Currently, it includes a **Links Manager** and a **Code Snippets Manager**, with an **Expense Tracker** in progress and a **To-Do List Manager** planned for future releases.

This app is built using **Blazor**, **ASP.NET Core**, and **SQL Server**.

![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Sathiyaraman-M/Personals/.github%2Fworkflows%2Fbuild-and-publish.yaml) 
[![Docker Image Version](https://img.shields.io/github/tag/Sathiyaraman-M/personals.svg)](https://github.com/Sathiyaraman-M/Personals/tags)  
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

---

## Installation

Clone the repository to your local machine using the following command:

```bash
git clone https://github.com/Sathiyaraman-M/personals.git
```

Navigate to the `local` directory and run the Docker Compose command to start the whole application:

```bash
cd personals/local
docker-compose up -d
```

This will start the application at `http://localhost:7005`

---

## Using the application

After setting up the application, you can access it at `http://localhost:7005`. 
The default username is `Admin` and the password is `Admin@123`.

> In production environments, it is recommended to change the default password.

You can interact with the following features:
- **Links Manager**: Store and categorize your links for easy access.
- **Code Snippets Manager**: Save and retrieve reusable code snippets for your projects.

### Upcoming Features:
- **Expense Tracker**: Track and categorize your personal expenses. (Work-in-progress)
- **To-Do List Manager**: Manage tasks and to-dos. (Planned)

---

## License

This project is licensed under the **MIT License** - see the [LICENSE.md](LICENSE.md) file for details.