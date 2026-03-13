preciso que me ajudes a fazer um pipeline de ci, vou te mandar o projeto so para teres uma ideia:

Neste projeto o grupo vai ser desafiado a fazer um sistema completo from start to finish, com a publicação em
app stores e go live. Para isso vão usar LLMs para gerar o código enquanto garantem a qualidade de todos os
9
componentes produzidos.
Propomos a criação de uma aplicação mobile para gestão de listas de compras, pensada para uso rápido em
contexto real (ex.: supermercado) e respetivo ecossistema de microserviços formalizando APIs de backend e
web app para backoffice.
A solução permite selecionar produtos, marcar itens durante a compra e consultar preços associados a
diferentes lojas e localizações, com a possibilidade de definir alertas de preço para determinados produtos.
O sistema inclui mecanismos de gestão central de dados (produtos, preços, lojas) e uma componente de push
notifications para alertas da alteração da lista.
Vão ainda demonstrar um conceito de White Label development, no qual o desenvolvimento das Apps mobile
e web serão baseadas num Design System que será customizado num segundo pipeline.
Tecnologias
• React Native
• Leitura de códigos de barras através da câmara
• Georreferenciação de preços
• Aplicações web em React.
• Backend com Serviços com API REST bem definidas e domínios bem delimitados construídos em
dotnet.
• Integração com o Keycloak como Identity Provider para configuração de utilizadores e integração
com outros IdP.
• Push Notifications integrado com Firebase Cloud Messaging
• CI/CD
Dois pipelines de CI/CD:
Build, deploy e testing da versão White Label
Customize, build, deploy e testing da versão customizada.
Deploy em ambiente público
Publicação para app stores (Android e IOS)
• Uso de AI LLMs como apoio ao desenvolvimento e validação – Plataforma Github Copilot
integrada com VS Code.
Este projeto cruza desenvolvimento mobile, web, backend e dados, obrigando a lidar com temas como
performance, sincronização, modelação de dados e fiabilidade de alertas. É indicado para alunos que querem
compreender como aplicações simples podem escalar para serviços digitais completos e sustentáveis.

pronto, o que eu quero fazer, estou a inicar agora e estou a começar por um serviço indepente e quero primeiro criar o artefact, vou usar o dotnet para o serviço, achas que consegues fazer um pipeline ci apenas para criar o artifact ?



pergunta adicional: 

 - onde o artefact é criado ? e o porque ?
 - R: 
        No pipeline que te mostrei, o artifact é criado neste passo:

        - name: Publish
        run: dotnet publish -c Release -o publish

        Aqui o .NET cria uma pasta chamada:

        publish/

        Dentro dessa pasta ficam os ficheiros compilados da aplicação.

        Exemplo do que aparece lá:

        publish/
        ├─ ShoppingService.dll
        ├─ ShoppingService.runtimeconfig.json
        ├─ ShoppingService.deps.json
        └─ appsettings.json

        Esses ficheiros são a versão compilada pronta a correr.

        ☁️ Onde ele fica depois no CI:

        Depois deste passo:

        - name: Upload artifact
        uses: actions/upload-artifact@v4
        with:
            name: shopping-service
            path: publish

        o GitHub Actions pega nessa pasta e:

        1️⃣ faz compressão
        2️⃣ guarda nos artifacts do workflow

        No GitHub vais encontrar em:

        GitHub Repo
        → Actions
        → Run do pipeline
            → Artifacts
                shopping-service

        Normalmente aparece como um .zip.
