window.onload = async () => {
    const response = await fetch('/api/chat/welcome');
    const welcomeMessage = await response.text();
    addMessageToChat(welcomeMessage, "ai");
};

document.getElementById('submitCommand').addEventListener('click', async () => {
    const command = document.getElementById('commandInput').value;
    addMessageToChat(command, "user");

    const response = await fetch('/api/chat/ask', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(command)
    });
    const result = await response.text();

    if (result.startsWith('[')) {
        const formattedResult = formatContacts(result);
        addMessageToChat(formattedResult, "ai");
    } else {
        addMessageToChat(result, "ai");
    }
    commandInput.value = '';
});

function formatContacts(contactData) {
    let formattedData = '<ul>';

    const contacts = JSON.parse(contactData);
    contacts.forEach(contact => {
        formattedData += `<li><strong>Name:</strong> ${contact.name} <br><strong>Phone:</strong> ${contact.phoneNumber}</li>`;
    });

    formattedData += '</ul>';
    return formattedData;
}

function addMessageToChat(message, sender) {
    const chatBox = document.getElementById("chatBox");
    const bubble = document.createElement("div");
    bubble.classList.add("chat-bubble", sender);

    if (message.includes("<ul>") || message.includes("<li>")) {
        bubble.innerHTML = message;
    } else {
        bubble.textContent = message;
    }

    chatBox.appendChild(bubble);
    chatBox.scrollTop = chatBox.scrollHeight;
}

document.getElementById('submitPrompt').addEventListener('click', async () => {
    const prompt = document.getElementById('promptInput').value;

    try {
        const response = await fetch('/api/chat/ask', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ prompt: prompt })
        });

        if (response.ok) {
            const result = await response.json();
            document.getElementById('aiResponse').textContent = result.message;
        } else {
            document.getElementById('aiResponse').textContent = 'Error: ' + response.statusText;
        }
    } catch (error) {
        document.getElementById('aiResponse').textContent = 'Error: ' + error.message;
    }
});
