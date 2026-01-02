# 🧠 WorkNet  
### Internal Work & Project Management System (LAN-Based Demo)

WorkNet is a **desktop-based internal work and project management application** designed for **small and private companies** that prefer working on a **local network without internet dependency**.

This project is built as a **realistic, daily-usable demo**, focusing on practical software architecture, offline-first design, and internal network collaboration.  
Although it is presented as a demo, the application is fully functional and can be used in real daily workflows inside local environments.

> ⚠️ This repository is intended as a **portfolio & demo project**.  
> The software may contain minor bugs or edge cases and is not intended for production-scale deployment.

---

## ✨ Key Highlights

- 🖥️ Desktop application (Offline-first)
- 🌐 Works on **Local Network (LAN)** only
- 🏢 Suitable for **small & private companies**
- 📊 Reads structured data from Excel files
- 🗃️ SQLite-based user management
- 👥 Online user detection inside LAN
- 📤 Internal file sending & receiving
- 📝 Daily reports system
- 🔔 Reminders & notifications
- 🔄 Auto-update support (if a newer version exists)
- 🧩 Designed for real daily internal usage

---

## 🎯 What WorkNet Does

WorkNet helps teams manage their internal workflow **without cloud services or internet access**.

The application operates entirely inside a **local network** and uses shared folders to simulate a lightweight server-client environment, similar to internal file-sharing systems.

Key capabilities include:

- Reading **companies and projects** from predefined Excel files
- Storing users and roles in a **SQLite database**
- Managing internal users (Admin / Normal User)
- Detecting **online users** through network presence
- Sending and receiving files between users
- Collecting daily activity reports
- Showing reminders and system notifications
- Automatically switching to **Local Mode** when the server is unreachable

This approach makes WorkNet reliable, simple, and suitable for environments where cloud solutions are not desired.

---

## 🧩 Core Features

### 🔐 Authentication & Users
- Login and Register system
- Role-based access (Admin / User)
- User data stored locally using SQLite

### 🏢 Company & Project Management
- Companies loaded from Excel files
- Projects linked to companies
- Project status tracking:
  - Pending
  - In Progress
  - Near Finish
- Delivery dates and request numbers
- Clear project overview dashboard

### 📤 File Transfer System
- Internal file sending via LAN shared folders
- File receive notifications
- Ability to open received files instantly

### 📝 Daily Reports
- Users can submit daily reports
- Reports are timestamped
- Admins can review user activity easily

### 🌐 Network & Offline Logic
- Server IP is read from a configurable text file
- Fully functional without internet
- If the server is unreachable:
  - Application switches to **Local Mode**
  - Data is saved locally
  - User is notified automatically

### 🔄 Update Support
- The application can detect newer versions (if available)
- Supports receiving and running the latest update
- Keeps the demo usable for long-term daily usage

---

## 🛠️ Tech Stack

- **Language:** C# (.NET WinForms)
- **Database:** SQLite
- **Data Source:** Microsoft Excel (.xlsx)
- **Networking:** LAN File Sharing (UNC paths)
- **UI:** Custom Dark Modern UI
- **Architecture:** Offline-first Desktop Application

---

## 📁 Project Structure

