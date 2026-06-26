# Appointment Booking System

## Prerequisite

Install and open [Docker Desktop](https://www.docker.com/products/docker-desktop) before running any commands.

---

## Commands

**Start the app**
```bash
docker-compose up --build
```

**Stop the app**
```bash
docker-compose down
```

**Stop the app and delete the database**
```bash
docker-compose down -v
```

---

## URLs

| What          | URL                    | Notes                        |
|---------------|------------------------|------------------------------|
| Web App       | http://localhost:4200  | Main application             |
| Database UI   | http://localhost:8080  | Browse database (Adminer)    |
| RabbitMQ UI   | http://localhost:15672 | guest / guest                |

---

## Adminer (Database UI) Login

```
System:   MS SQL (beta)
Server:   sqlserver
Username: sa
Password: Sa@12345
Database: (leave empty)
```
