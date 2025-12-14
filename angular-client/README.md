# 🐰 RabbitMQ Demo - Frontend (Angular)

> **⚠️ ATTENZIONE: LEGGERE PRIMA DI PROCEDERE**
>
> Questo progetto Angular è stato **interamente generato tramite Intelligenza Artificiale** allo scopo esclusivo di fornire una UI funzionante per la demo.
>
> **IL CODICE SORGENTE DI QUESTO FRONTEND NON HA ALCUN VALORE DIDATTICO.**

## 🚫 Disclaimer Importante
1.  **Codice "Usa e Getta":** Non analizzare, non giudicare e non prendere spunto dal codice TypeScript/HTML contenuto in questa cartella. Non segue best practices, pattern architetturali o standard di qualità. È stato generato solo per avere dei "bottoni da cliccare".
2.  **RabbitMQ non è qui:** Questo frontend **NON** contiene alcuna logica relativa a RabbitMQ. Angular non comunica con RabbitMQ.
3.  **Funzionamento:** Questa interfaccia si limita a inviare semplici chiamate HTTP POST alle API del Backend (.NET). È il Backend che gestisce tutta la logica di code, exchange e consumer.

## 🎯 Scopo del Progetto
L'unico scopo di questa interfaccia è visivo:
1.  Premere un pulsante per scatenare il carico di lavoro.
2.  Mostrare la differenza visiva (UI bloccata vs UI fluida) tra una chiamata Sincrona e una Asincrona gestita con code.

## 🚀 Come avviare (se proprio devi)

Se devi avviare la UI per testare il backend:

```bash
# 1. Installa le dipendenze
npm install

# 2. Avvia il server di sviluppo
npm start
