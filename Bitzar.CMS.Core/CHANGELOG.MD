# Changelog
As alterações realizadas no Bitzar.CMS são registradas nesse arquivo.  

O formato desse _changelog_ segue o padrão disposto em [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)
e o versionamento segue o padrão de [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.9.1 - 2022-03-14](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.9.1.api)
### [Fix]
- Correção no retorno de variáveis globais para que não estavam retornando os values do tipo texto via Api.

## [2.9.0 - 2022-03-06](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.9.0.api)
### [Feature]
- Adicionado mecanismo para substituição das páginas Internas do CMS.
- Atualização para o .Net Framework 4.8

## [2.8.4 - 2022-02-27](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.8.5.api)
### [Feature]
- Adicionado funcionalidade de reordenação dos repetidores na edição de conteúdo.
- Adicionado no Login validação se compilação é DEBUG ou RELEASE.

### [Fix]
- Largura do Header do painel quando tem uma nova notificação.

## [2.8.3 - 2022-02-04](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.8.3.api)
### [Fix]
- Correção redirecionamento da página 404 - Página não encontrada.

## [2.8.2 - 2022-01-25](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.8.2.api)
### [Fix]
- Alteração para evitar concorrencia no dicionário de cache do CMS. (Bitzar.CMS.Extension)
- Correção no parser do ID de Mídia no Global (Functions.Global)
- Padronização das versões das DLL's (Core.dll, Extension.dll, Model.dll, Data.dll)

## [2.8.1 - 2021-12-29](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.8.1.api)
### [Added]
- Adicionado funcionalidade para limpeza das rotas no cache

## [2.8.0 - 2021-11-26](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.8.0.api)
### [Added]
- Funcionalidade para prevenir o carregamento de plugins em modo Debug
- Adicionado configuração para exibir se o site esta em modo DEBUG ou Não.
- Adicionado no footer informações do Tipo de DB, Servidor, Banco de Dados e Modo de Execução
- Corrigido carregamento do Layout para sobrescrever arquivos do CMS.

## [2.7.2 - 2021-10-15](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.7.2.api)
### [Added]
- Adicionado funcionalidade para adicionar um userfield ao usuario.

## [2.7.1 - 2021-10-15](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.7.1.api)
### [Added]
- Adicionado funcionalidade para armazenamento do Branding.js e Branding.css dentro do CMS como arquvios editáveis na base

## [2.6.0 - 2020-08-07](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.6.0.api)
### [Added]
- Adicionado funcionalidade de geração do SiteMap como Feature Core do CMS.

## [2.5.1 - 2020-08-03](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.5.1.api)
### [Fixed]
- Corrigido bug na seleção de imagens dentro do Html Editor.

## [2.5.0 - 2020-05-19](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.5.0.api)
### [Added]
- Implementação autenticação OAuth

## [2.4.2 - 2020-05-19](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.4.2.api)
### [Fix]
- Adicionado arquivos essenciais do Layout que haviam sido removidos do Repositorio. (_ViewStart.cshtml e web.config)

### [Added]
- Adicionado compatibilidade com arquivos .webp.

## [2.4.1 - 2020-05-19](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.4.1.api)
### [Fix]
- DatabaseConnection:
	- Correção da definição de CommandTimeOut na conexão;

## [2.4.0 - 2020-05-18](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.3.0.api)
### [Added]
- Autenticação:
	- Criação e configuração da tabela Device;

## [2.3.0 - 2020-05-18](https://bitzar@dev.azure.com/bitzar/BitzarSolutions/_git/Bitzar-CMS?version=GTv2.3.0.api)
### [Added]
- Configuração de API Rest para consumo dos endpoints:
	- Blog;
	- Global;
	- Language;
	- Library;
	- Plugin;
	- Template e
	- Text
- Criação de Helpers para apoio dos Controllers;
- Configuração de time-out para conexão com o Banco de Dados.