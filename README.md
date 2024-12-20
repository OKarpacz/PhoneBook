# PhoneBook AI 📞

## Table of Contents
1. [Project Overview](#project-overview)
2. [Key Features](#key-features)
3. [Example Prompts](#example-prompts)
4. [Installation](#installation)
5. [Disclaimer](#disclaimer)
6. [License](#license)

---

## Project Overview

**PhoneBook AI** is a contact management tool with built-in AI capabilities to enhance the user experience. This project is designed to help users add, update, search, and manage contacts within a secure and efficient application.

## Key Features

- **Contact Management**: Add, edit, delete, and search for contacts with ease.
- **AI-Powered Insights**: Utilizes AI to provide enhanced recommendations or search capabilities.
- **Responsive Design**: User-friendly interface that works on desktop and mobile.
- **Data Security**: Run the application locally to maintain control over your data.
- **Extensibility**: Modular service structure allows for easy addition of features or integrations.

## Example Prompts

- "Add to book John. His phone number is 123456789."
- "Please add record to my phone book for Joanna with number 222333444"
- "What is the phone number for Joanna?"
- "Show me my contact list."
- "Edit John phone number to 987654321."
- "Delete John"

## Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/OKarpacz/PhoneBook.git
   cd PhoneBook
2. **Restore dependencies**:
   ```bash
   dotnet restore
3. **Build the project**:
   ```bash
   dotnet build
4. **Run the application**:
   ```bash
   dotnet run --project PhoneBook

## Disclaimer

There is a possibility that you need to create your own .env file for the api to work.

1. **Install a Library for .env File Parsing**:
   ```bash
   dotnet add package dotenv.net
   ```
2. **Change the OPENAI_API_KEY variable to your api key**:
   ```bash
   OPENAI_API_KEY="your-secret-api-key"
   ```
## License
This project is licensed under the MIT License. You are free to use, modify, and distribute this application.
