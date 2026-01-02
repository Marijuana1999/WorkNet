# 🧠 WorkNet  
### Internal Work & Project Management System (LAN-Based Demo)

WorkNet is a **desktop-based internal work and project management application** designed for **small and private companies** that prefer working on a **local network without internet dependency**.

This project is built as a **realistic, daily-usable demo**, focusing on practical software architecture, offline-first design, and internal network collaboration.  
Although presented as a demo, the application is fully functional and suitable for real daily internal usage.

> ⚠️ This repository is intended as a **portfolio & demo project**.  
> Minor bugs or edge cases may exist.

---

## ✨ Key Highlights

- 🖥️ Desktop application (Offline-first)
- 🌐 Works on **Local Network (LAN)** only
- 🏢 Suitable for **small & private companies**
- 📊 Excel-based data source
- 🗃️ SQLite user database
- 👥 Online user detection
- 📤 Internal file sending & receiving
- 📝 Daily reports system
- 🔔 Reminders & notifications
- 🔄 Auto-update support (if available)

---

## 🎯 What WorkNet Does

WorkNet enables teams to manage their internal workflow **without cloud services or internet access**.

The system operates entirely on a **local network**, using shared folders to simulate a lightweight server-client model similar to internal file-sharing systems.

Core capabilities include:
- Reading companies and projects from Excel files
- Managing users via SQLite
- Detecting online users in LAN
- Internal file exchange
- Daily reporting
- Offline-safe local mode

---

## 🧩 Core Features

### 🔐 Authentication
- Login & Register system
- Admin / User roles
- SQLite-based storage

### 🏢 Company & Project Management
- Excel-driven company and project lists
- Project status tracking
- Delivery dates & request numbers

### 📤 File Transfer
- LAN-based file sending
- Receive notifications
- Instant file opening

### 📝 Daily Reports
- User activity reports
- Admin review access
- Timestamped entries

### 🌐 Network Logic
- Server IP read from text file
- Automatic **Local Mode** if server is unreachable

---

## 🛠️ Tech Stack

- **Language:** C# (.NET WinForms)
- **Database:** SQLite
- **Data Source:** Excel (.xlsx)
- **Networking:** LAN File Sharing
- **UI:** Custom Dark Modern UI

---

## 📁 Project Structure


---

## 📸 Application Screenshots

### 🔐 Login
![Login](assets/screenshots/login.png)

---

### 📝 Register
![Register](assets/screenshots/register.png)

---

### 🏢 Companies
![Companies](assets/screenshots/companies.png)

---

### 📊 Projects
![Projects](assets/screenshots/projects.png)

---

### 📤 Send File
![Send File](assets/screenshots/send-file.png)

---

### 📥 Received File
![Received File](assets/screenshots/received-file.png)

---

### 📝 Daily Report
![Daily Report](assets/screenshots/daily-report.png)

---

### 🌐 Local Mode (Server Offline)
![Local Mode](assets/screenshots/local-mode.png)

---

## 🚀 Installation / How to Run

### Option 1: Run Executable
1. Download `WorkNet.exe` from Releases
2. Place Excel files inside `sample-data`
3. Set server IP in `config/server_ip.txt`
4. Run application inside LAN

### Option 2: Run From Source
```bash
git clone https://github.com/your-username/WorkNet.git
