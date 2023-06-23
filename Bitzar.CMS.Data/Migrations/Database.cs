using Bitzar.CMS.Data.Model;
using System;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Bitzar.CMS.Data
{
    public class Database
    {
        public static void Seed()
        {
            try
            {
                using (var context = new DatabaseConnection())
                {
                    #region Setup Default Configuration
                    var config = context.Configurations.ToList();

                    /* Internal System Configuration */
                    if (!config.Any(c => c.Id == "ProfileImagePath"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "ProfileImagePath",
                            Name = "Caminho de imagens de perfil",
                            Description = "Local para armazenamento físico das imagens de perfil dos usuários.",
                            Value = "~/content/library/users",
                            Order = 0,
                            System = true,
                            Category = "Interno",
                            Type = "text"
                        });

                    /* System default Configuration */
                    if (!config.Any(c => c.Id == "SiteName"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "SiteName",
                            Name = "Nome do Site",
                            Description = "Nome do Site para identificação.",
                            Value = "Novo Site",
                            Order = 1
                        });
                    if (!config.Any(c => c.Id == "SiteDescription"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "SiteDescription",
                            Name = "Breve Descrição do Site",
                            Description = "Descrição breve do seu site que poderá ser adicionada no SEO principal.",
                            Value = "Novo site criado pela plataforma Bitzar CMS",
                            Order = 2
                        });
                    if (!config.Any(c => c.Id == "DefaultLanguage"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "DefaultLanguage",
                            Name = "Idioma Padrão",
                            Description = "Definição de qual a cultura padrão do site.",
                            Value = "pt-BR",
                            Order = 3,
                            Type = "select",
                            Source = "language"
                        });
                    if (!config.Any(c => c.Id == "AllowedEmails"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "AllowedEmails",
                            Name = "E-mails autorizados para notificações",
                            Description = "Lista de e-mails (separada por ;) para validar o envio de notificações. Se vazio, permite o envio para qualquer destinatário.",
                            Value = "",
                            Order = 4
                        });
                    if (!config.Any(c => c.Id == "KeepStatisticsPeriod"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "KeepStatisticsPeriod",
                            Name = "Manter as estatísticas de acesso por N dias",
                            Description = "Indica que as estatísticas do sistema serão armazenadas por N dias.",
                            Value = "30",
                            Order = 5,
                            Type = "number"
                        });
                    if (!config.Any(c => c.Id == "ApiToken"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "ApiToken",
                            Name = "Token de Autenticação",
                            Description = "Token de Autenticação para Acesso externo aos plugins e outras API que podem ser públicas no sistema.",
                            Value = Data.Configuration.CreateToken(),
                            Order = 6,
                            Type = "text"
                        });
                    if (!config.Any(c => c.Id == "DefaultUrl"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "DefaultUrl",
                            Name = "Url Padrão",
                            Description = "Definição da url padrão do site.",
                            Value = "",
                            Order = 7
                        });
                    if (!config.Any(c => c.Id == "DevelopmentMode"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "DevelopmentMode",
                            Name = "Modo Desenvolvimento/Homologação?",
                            Description = "Apresenta um recurso visual no site para modo desenvolvimento ou homologação.",
                            Value = "false",
                            Type = "checkbox",
                            Order = 8
                        });

                    // Api Configuration
                    if (!config.Any(c => c.Id == "OAuthExpirationTime"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "OAuthExpirationTime",
                            Name = "Expiração do Token OAuth (api)",
                            Description = "Define o período de expiração padrão do Token OAuth em Horas. Padrão 168 horas (7 dias).",
                            Value = "168",
                            Order = 1,
                            Type = "number",
                            Category = "Api",
                        });
                    if (!config.Any(c => c.Id == "EnsureIsAuthenticated"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "EnsureIsAuthenticated",
                            Name = "Assegurar acesso autenticado na API",
                            Description = "Se marcado (true), exige que o acesso a API seja autenticado.",
                            Value = "true",
                            Order = 2,
                            Type = "checkbox",
                            Category = "Api",
                        });

                    /* Security Group */
                    if (!config.Any(c => c.Id == "EnforceSSL"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "EnforceSSL",
                            Name = "Força o uso de Certificado SSL",
                            Description = "Ao acessar a URL o tráfego será redirecionado para HTTPS se uso em HTTP.",
                            Value = "false",
                            Order = 0,
                            Category = "Segurança",
                            Type = "checkbox"
                        });
                    if (!config.Any(c => c.Id == "EnforceCaptcha"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "EnforceCaptcha",
                            Name = "Força o uso do Captcha",
                            Description = "Exige validação do Captcha do Google (reCaptcha).",
                            Value = "false",
                            Order = 1,
                            Category = "Segurança",
                            Type = "checkbox"
                        });
                    if (!config.Any(c => c.Id == "CaptchaSiteKey"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "CaptchaSiteKey",
                            Name = "Captcha Site Key",
                            Description = "Chave do Captcha para validação Client-Side.",
                            Value = "",
                            Order = 2,
                            Category = "Segurança",
                            Type = "text"
                        });
                    if (!config.Any(c => c.Id == "CaptchaSecretKey"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "CaptchaSecretKey",
                            Name = "Captcha Secret Key",
                            Description = "Chave Secreta do Captcha para validação no Server-Side.",
                            Value = "",
                            Order = 3,
                            Category = "Segurança",
                            Type = "text"
                        });
                    if (!config.Any(c => c.Id == "CaptchaValidationSite"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "CaptchaValidationSite",
                            Name = "Captcha Validation Site",
                            Description = "Site de Valição do reCaptcha Google. Não alterar.",
                            Value = "https://www.google.com/recaptcha/api/siteverify",
                            Order = 0,
                            System = true,
                            Category = "Segurança",
                            Type = "text"
                        });
                    if (!config.Any(c => c.Id == "TokenRequestTime"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "TokenRequestTime",
                            Name = "Token Request Time",
                            Description = "Tempo para expiração de um request.(Em minutos)",
                            Value = "60",
                            Order = 4,
                            Category = "Segurança",
                            Type = "text"
                        });

                    /* Throttling Configuration */
                    if (!config.Any(c => c.Id == "ThrottlingInstant"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "ThrottlingInstant",
                            Name = "Quantidade de Requisições máximas simultâneas.",
                            Description = "Quantidade de Requisições permitidas simultâneas.",
                            Value = "5",
                            Order = 1,
                            Type = "number",
                            Category = "Throttling",
                        });
                    if (!config.Any(c => c.Id == "ThrottlingMinute"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "ThrottlingMinute",
                            Name = "Quantidade de Requisições máximas dentro do período de 1 minuto.",
                            Description = "Quantidade de Requisições máximas permitidas dentro de 1 minuto.",
                            Value = "30",
                            Order = 2,
                            Type = "number",
                            Category = "Throttling",
                        });
                    if (!config.Any(c => c.Id == "ThrottlingHour"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "ThrottlingHour",
                            Name = "Quantidade de Requisições máximas dentro do período de 1 hora.",
                            Description = "Quantidade de Requisições máximas permitidas dentro de 1 hora.",
                            Value = "500",
                            Order = 3,
                            Type = "number",
                            Category = "Throttling",
                        });
                    if (!config.Any(c => c.Id == "ThrottlingDay"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "ThrottlingDay",
                            Name = "Quantidade de Requisições máximas dentro do período de 1 dia.",
                            Description = "Quantidade de Requisições máximas permitidas dentro de 1 dia.",
                            Value = "2000",
                            Order = 4,
                            Type = "number",
                            Category = "Throttling",
                        });
                    if (!config.Any(c => c.Id == "ThrottlingAuthUsersMultiplier"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "ThrottlingAuthUsersMultiplier",
                            Name = "Aumentar quantidade de requisições de usuários autenticados.",
                            Description = "Aumenta a capacidade de requisições de usuários autenticados N vezes de forma global.",
                            Value = "2",
                            Order = 5,
                            Type = "number",
                            Category = "Throttling",
                        });

                    /* Authentication Configuration */
                    if (!config.Any(c => c.Id == "MembershipEnabled"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "MembershipEnabled",
                            Name = "Controle de Acesso Habilitado",
                            Description = "Indica se o controle de acesso está habilitado no sistema.",
                            Value = "false",
                            Category = "Controle de Acesso",
                            Order = 1,
                            Type = "checkbox"
                        });
                    if (!config.Any(c => c.Id == "ExpirationTime"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "ExpirationTime",
                            Name = "Tempo Expiração",
                            Description = "Define em minutos o tempo de expiração da sessão do usuário autenticado",
                            Value = "60",
                            Category = "Controle de Acesso",
                            Order = 2,
                            Type = "number"
                        });
                    if (!config.Any(c => c.Id == "UseSlidingExpiration"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "UseSlidingExpiration",
                            Name = "Renovar tempo de expiração",
                            Description = "Se marcado, a cada solicitação no site o tempo de expiração é renovado.",
                            Value = "true",
                            Category = "Controle de Acesso",
                            Order = 3,
                            Type = "checkbox"
                        });
                    if (!config.Any(c => c.Id == "LoginPage"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "LoginPage",
                            Name = "Página de Login",
                            Description = "Define a página principal de Login para executar o redirect Automático",
                            Value = "Login.cshtml",
                            Category = "Controle de Acesso",
                            Order = 4,
                            Type = "select",
                            Source = "page"
                        });
                    if (!config.Any(c => c.Id == "AlterPasswordPage"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "AlterPasswordPage",
                            Name = "Página de Alteração de Senha",
                            Description = "Define a página de alteração de Senha para tratamento automático",
                            Value = "AlterarSenha.cshtml",
                            Category = "Controle de Acesso",
                            Order = 5,
                            Type = "select",
                            Source = "page"
                        });
                    if (!config.Any(c => c.Id == "ProfilePage"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "ProfilePage",
                            Name = "Página de dados do Perfil do Usuário",
                            Description = "Define a página de alteração dos dados do perfil do usuário para redirecionamento automático",
                            Value = "Perfil.cshtml",
                            Category = "Controle de Acesso",
                            Order = 6,
                            Type = "select",
                            Source = "page"
                        });
                    if (!config.Any(c => c.Id == "AutoScaleProfilePicture"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "AutoScaleProfilePicture",
                            Name = "Otimizar automaticamente a imagem do perfil",
                            Description = "Indicar o tamanho máximo de altura ou largura em pixels para o arquivo final. Vazio ou zero para desabilitar.",
                            Value = "200",
                            Category = "Controle de Acesso",
                            Order = 7,
                            Type = "number"
                        });
                    if (!config.Any(c => c.Id == "AllowMembershipManagement"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "AllowMembershipManagement",
                            Name = "Permite gerenciamento dos Membros",
                            Description = "Se marcado, usuários não Administradores podem gerenciar os membros",
                            Value = "false",
                            Category = "Controle de Acesso",
                            Order = 8,
                            Type = "checkbox"
                        });
                    if (!config.Any(c => c.Id == "RequiredEmailToBeValidated"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "RequiredEmailToBeValidated",
                            Name = "Requer que o e-mail esteja validado antes do login.",
                            Description = "Se marcado, caso o e-mail não estiver validado nega o login e solicita validação. Se não, permite o login.",
                            Value = "true",
                            Category = "Controle de Acesso",
                            Order = 9,
                            Type = "checkbox"
                        });
                    if (!config.Any(c => c.Id == "DefaultMemberRole"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "DefaultMemberRole",
                            Name = "Indica o perfil padrão de novos membros cadastrados.",
                            Description = "Ao criar ou importar um novo usuário e não especificar o perfil, este será o padrão utilizado.",
                            Value = "",
                            Category = "Controle de Acesso",
                            Order = 10,
                            Type = "select",
                            Source = "role"
                        });
                    if (!config.Any(c => c.Id == "AutoLoginOnMemberValidate"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "AutoLoginOnMemberValidate",
                            Name = "Auto-Login ao Validar Membro.",
                            Description = "Ao validar o cadastro pelo e-mail, o membro é automaticamente logado no sistema.",
                            Value = "false",
                            Category = "Controle de Acesso",
                            Order = 11,
                            Type = "checkbox"
                        });

                    /* Email COnfiguration */
                    if (!config.Any(c => c.Id == "SmtpHost"))
                    {
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "SmtpHost",
                            Name = "Endereço de E-mail SMTP",
                            Description = "Indica o endereço de saída para envio de mensagens de E-mail SMTP",
                            Value = "",
                            Category = "Notificação E-mail",
                            Order = 0,
                            Type = "text"
                        });
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "SmtpPort",
                            Name = "Porta de Saída SMTP",
                            Description = "Porta de comunicação com o servidor de E-mail. Pode ser necessário liberação em sistemas de Firewall.",
                            Value = "587",
                            Category = "Notificação E-mail",
                            Order = 1,
                            Type = "number"
                        });
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "SmtpSender",
                            Name = "Endereço de E-mail do Remetente",
                            Description = "E-mail da conta de saída padrão do SMTP.",
                            Value = "",
                            Category = "Notificação E-mail",
                            Order = 2,
                            Type = "text"
                        });
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "SmtpDisplayName",
                            Name = "Nome de Exibição do Remetente",
                            Description = "Valor de Exibição para o E-mail do Remetente no cliente de e-mail.",
                            Value = "",
                            Category = "Notificação E-mail",
                            Order = 3,
                            Type = "text"
                        });
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "SmtpUser",
                            Name = "Nome do Usuário do servidor SMTP",
                            Description = "Nome do Usuário de Autenticação no servidor de SMTP.",
                            Value = "",
                            Category = "Notificação E-mail",
                            Order = 4,
                            Type = "text"
                        });
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "SmtpPassword",
                            Name = "Senha do Usuário do servidor SMTP",
                            Description = "Senha do Usuário de Autenticação no servidor de SMTP.",
                            Value = "",
                            Category = "Notificação E-mail",
                            Order = 5,
                            Type = "password"
                        });
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "SmtpSSL",
                            Name = "Comunicação Segura?",
                            Description = "Indica que a comunicação com o servidor de e-mails será realizada de forma Criptografada.",
                            Value = "true",
                            Category = "Notificação E-mail",
                            Order = 6,
                            Type = "checkbox"
                        });
                    }
                    /* Blog Configuration */
                    if (!config.Any(c => c.Id == "BlogPostPage"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "BlogPostPage",
                            Name = "Página padrão de Postagem do Blog",
                            Description = "Página padrão com o template de exibição dos dados da Postagem do Blog",
                            Value = "Blog.cshtml",
                            Category = "Configuração do Blog",
                            Order = 0,
                            Type = "select",
                            Source = "page"
                        });
                    if (!config.Any(c => c.Id == "SplitBlogCategory"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "SplitBlogCategory",
                            Name = "Separar categorias do Blog no Menu",
                            Description = "Separa as categorias do Blog como items do menu de acesso rápido",
                            Value = "true",
                            Category = "Configuração do Blog",
                            Order = 1,
                            Type = "checkbox"
                        });

                    /* Other Configuration */
                    if (!config.Any(c => c.Id == "LibraryFixedMessage"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "LibraryFixedMessage",
                            Name = "Mensagem de Alerta fixa na Biblioteca",
                            Description = "Indica uma mensagem de alerta fixa para instrução geral dentro da Biblioteca de Uploads",
                            Value = "",
                            Category = "Outros",
                            Order = 0,
                            Type = "text",
                        });
                    if (!config.Any(c => c.Id == "RepeaterReorderForAllLanguages"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "RepeaterReorderForAllLanguages",
                            Name = "Reordenar Repetidores para todos os idiomas",
                            Description = "Ao reordenar um repetidor, todos os idiomas também serão reordenados. Se estiver desmarcado, apenas o idioma selecionado será reordenado.",
                            Value = "true",
                            Category = "Outros",
                            Order = 1,
                            Type = "checkbox",
                        });

                    if (!config.Any(c => c.Id == "MinifyCssAndJs"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "MinifyCssAndJs",
                            Name = "Comprimir arquivos JS e CSS para melhorar performance",
                            Description = "Executa rotinas de minification dos arquivos JS e CSS para aumentar a performance de carregamento dos dados",
                            Value = "true",
                            Category = "Otimização",
                            Order = 1,
                            Type = "checkbox",
                        });
                    /* Performance Configuration */
                    if (!config.Any(c => c.Id == "OptimizeImages"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "OptimizeImages",
                            Name = "Otimizar arquivos de imagem para melhoria de performance.",
                            Description = "Indica se deve utilizar serviço de otimização de imagens.",
                            Value = "false",
                            Category = "Otimização",
                            Order = 2,
                            Type = "checkbox",
                        });
                    if (!config.Any(c => c.Id == "OptimizeImagesToken"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "OptimizeImagesToken",
                            Name = "Token do serviço de otimização de imagens.",
                            Description = "Token de configuração para o serviço de otimização TinyPng. https://tinypng.com/developers.",
                            Value = "",
                            Category = "Otimização",
                            Order = 3,
                            Type = "text",
                        });
                    /*Azure Storage Configuration*/
                    if (!config.Any(c => c.Id == "AzureStorage"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "AzureStorage",
                            Name = "Ativar Storage no AzureBlob.",
                            Description = "Indica se deve armazenar os arquivos no BlobStorage.",
                            Value = "false",
                            Category = "Otimização",
                            Order = 4,
                            Type = "checkbox",
                        });
                    if (!config.Any(c => c.Id == "AzureStorageKey"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "AzureStorageKey",
                            Name = "Key para conexão com AzureBlob.",
                            Description = "Indica a chave de conexão com o AzureStorage.",
                            Value = "",
                            Category = "Otimização",
                            Order = 5,
                            Type = "text",
                        });
                    if (!config.Any(c => c.Id == "AzureStorageUrl"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "AzureStorageUrl",
                            Name = "Url de acesso ao AzureBlob.",
                            Description = "Indica a url de acesso ao AzureStorage.",
                            Value = "",
                            Category = "Otimização",
                            Order = 6,
                            Type = "text",
                        });
                    if (!config.Any(c => c.Id == "AzureStorageContainer"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "AzureStorageContainer",
                            Name = "Container de conexão dentro do blob.",
                            Description = "Indica qual o nome do container que irá se conectar ou criar caso não exista dentro do blob.",
                            Value = "",
                            Category = "Otimização",
                            Order = 7,
                            Type = "text",
                        });
                    if (!config.Any(c => c.Id == "AzureStorageContainerForLogs"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "AzureStorageContainerForLogs",
                            Name = "Container padrão para salvar logs no storage",
                            Description = "Indica o container do AzureStorage para salvar logs",
                            Value = "",
                            Category = "Otimização",
                            Order = 8,
                            Type = "text",
                        });


                    /* SEO Configuration */
                    if (!config.Any(c => c.Id == "GenerateSiteMap"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "GenerateSiteMap",
                            Name = "Habilitar geração do arquivo sitemap.xml",
                            Description = "Habilita a geração automática do arquivo SiteMap automaticamente.",
                            Value = "true",
                            Category = "SEO",
                            Order = 10,
                            Type = "checkbox",
                        });
                    if (!config.Any(c => c.Id == "GenerateSiteMapOnSave"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "GenerateSiteMapOnSave",
                            Name = "Gerar SiteMap automaticamente ao salvar um novo conteúdo",
                            Description = "Sempre que uma nova versão do conteúdo é salva, cria um novo arquivo de SiteMap automaticamente substintuindo o anterior.",
                            Value = "true",
                            Category = "SEO",
                            Order = 20,
                            Type = "checkbox",
                        });
                    if (!config.Any(c => c.Id == "GenerateSiteMapOnCacheClear"))
                        context.Configurations.Add(new Model.Configuration()
                        {
                            Id = "GenerateSiteMapOnCacheClear",
                            Name = "Gerar SiteMap automaticamente ao limpar o cache de dados",
                            Description = "Sempre a rotina de cache de dados é acionada, a função de geração do SiteMap será acionada.",
                            Value = "true",
                            Category = "SEO",
                            Order = 30,
                            Type = "checkbox",
                        });

                    #endregion

                    #region Setup Default Language pt-BR
                    if (!context.Languages.Any(l => l.Culture == "pt-BR"))
                        context.Languages.Add(new Language()
                        {
                            Culture = "pt-BR",
                            Description = "Português Brasileiro",
                            UrlRoute = null
                        });
                    #endregion

                    #region Setup User Roles
                    context.Roles.AddOrUpdate(
                        p => new { p.AdminRole, p.Name },
                        new Role()
                        {
                            Name = "Administrador",
                            Description = "Acesso total em todas as áreas do CMS. Permite gerenciar configurações, acesso, layout e conteúdo.",
                            AdminRole = true
                        },
                        new Role()
                        {
                            Name = "Designer",
                            Description = "Acesso parcial as áreas do CMS. Permite gerenciar layout e conteúdo.",
                            AdminRole = true
                        },
                        new Role()
                        {
                            Name = "Editor",
                            Description = "Acesso restrito no menu de conteúdo apenas.",
                            AdminRole = true
                        }
                    );

                    if (!context.RolesPermissions.Any())
                    {
                        context.RolesPermissions.AddOrUpdate(
                            p => new { p.IdRole, p.Source, p.Module, p.Function },
                            new RolePermission()
                            {
                                IdRole = 2,
                                Source = "Core",
                                Module = "Configurações",
                                Function = null,
                                Status = RolePermission.PermissionType.Allow
                            },
                            new RolePermission()
                            {
                                IdRole = 2,
                                Source = "Core",
                                Module = "Configurações",
                                Function = "Usuarios Administrativos",
                                Status = RolePermission.PermissionType.Allow
                            },
                            new RolePermission()
                            {
                                IdRole = 2,
                                Source = "Core",
                                Module = "Configurações",
                                Function = "Idiomas",
                                Status = RolePermission.PermissionType.Deny
                            },
                            new RolePermission()
                            {
                                IdRole = 2,
                                Source = "Core",
                                Module = "Configurações",
                                Function = "Plugins",
                                Status = RolePermission.PermissionType.Deny
                            },
                            new RolePermission()
                            {
                                IdRole = 2,
                                Source = "Core",
                                Module = "Configurações",
                                Function = "Configurações",
                                Status = RolePermission.PermissionType.Deny
                            },
                            new RolePermission()
                            {
                                IdRole = 3,
                                Source = "Core",
                                Module = "Configurações",
                                Function = null,
                                Status = RolePermission.PermissionType.Allow
                            },
                            new RolePermission()
                            {
                                IdRole = 3,
                                Source = "Core",
                                Module = "Configurações",
                                Function = "Usuarios Administrativos",
                                Status = RolePermission.PermissionType.Allow
                            },
                            new RolePermission()
                            {
                                IdRole = 3,
                                Source = "Core",
                                Module = "Configurações",
                                Function = "Idiomas",
                                Status = RolePermission.PermissionType.Deny
                            },
                            new RolePermission()
                            {
                                IdRole = 3,
                                Source = "Core",
                                Module = "Configurações",
                                Function = "Plugins",
                                Status = RolePermission.PermissionType.Deny
                            },
                            new RolePermission()
                            {
                                IdRole = 3,
                                Source = "Core",
                                Module = "Configurações",
                                Function = "Configurações",
                                Status = RolePermission.PermissionType.Deny
                            },
                            new RolePermission()
                            {
                                IdRole = 3,
                                Source = "Core",
                                Module = "Membership",
                                Function = null,
                                Status = RolePermission.PermissionType.Deny
                            },
                            new RolePermission()
                            {
                                IdRole = 3,
                                Source = "Core",
                                Module = "Design",
                                Function = null,
                                Status = RolePermission.PermissionType.Deny
                            },
                             new RolePermission()
                             {
                                 IdRole = 3,
                                 Source = "Core",
                                 Module = "Configurações",
                                 Function = "Log",
                                 Status = RolePermission.PermissionType.Deny
                             }
                        );
                    }
                    #endregion

                    #region  Setup Template Types Available
                    context.TemplateTypes.AddOrUpdate(
                        t => t.Name,
                        new TemplateType()
                        {
                            Name = "View",
                            DefaultPath = "~/Views/Main",
                            DefaultExtension = "cshtml",
                            Editor = "razor"
                        },
                        new TemplateType()
                        {
                            Name = "Layout",
                            DefaultPath = "~/Views/Shared",
                            DefaultExtension = "cshtml",
                            Editor = "razor"
                        },
                        new TemplateType()
                        {
                            Name = "Partial",
                            DefaultPath = "~/Views/Shared",
                            DefaultExtension = "cshtml",
                            Editor = "razor"
                        },
                        new TemplateType()
                        {
                            Name = "BlogPost",
                            DefaultPath = "~/Views/Blog",
                            DefaultExtension = "cshtml",
                            Editor = "razor"
                        },
                        new TemplateType()
                        {
                            Name = "StyleSheet",
                            DefaultPath = "~/content/css",
                            DefaultExtension = "css",
                            Editor = "css"
                        },
                        new TemplateType()
                        {
                            Name = "Javascript",
                            DefaultPath = "~/content/js",
                            DefaultExtension = "js",
                            Editor = "javascript"
                        },
                        new TemplateType()
                        {
                            Name = "Other",
                            DefaultPath = "~/content/other",
                            DefaultExtension = "*",
                            Editor = "text"
                        },
                        new TemplateType()
                        {
                            Name = "Template",
                            DefaultPath = "~/content/template",
                            DefaultExtension = "cshtml",
                            Editor = "razor"
                        });

                    #endregion

                    #region Setup Library Types
                    context.LibraryTypes.AddOrUpdate(
                        p => p.Description,
                        new LibraryType()
                        {
                            Description = "Image",
                            MimeTypes = "image/*",
                            AllowedExtensions = ".jpg,.jpeg,.gif,.png,.webp",
                            DefaultPath = "~/content/library/images",
                            IsImageType = true
                        },
                        new LibraryType()
                        {
                            Description = "Audio",
                            MimeTypes = "audio/*",
                            AllowedExtensions = ".aac,.m4a,.mp1,.mp2,.mp3,.mpg,.mpeg,.oga,.ogg,.wav",
                            DefaultPath = "~/content/library/audios"
                        },
                        new LibraryType()
                        {
                            Description = "Video",
                            MimeTypes = "video/*",
                            AllowedExtensions = ".mp4,.m4v,.ogv,.webm",
                            DefaultPath = "~/content/library/videos"
                        },
                        new LibraryType()
                        {
                            Description = "Other",
                            MimeTypes = "application/octet-stream",
                            AllowedExtensions = ".txt,.doc,.docx,.odt,.fodt,.xls,.xlsx,.ods,.fods,.ppt,.pptx,.pps,.ppsx,.odp,.fodp,.xml,.pdf,.xps,.zip,.rar,.7z,.jar,.gz,.psd,.ai,.cdr,.json",
                            DefaultPath = "~/content/library/files"
                        }
                    );
                    #endregion

                    #region Setup Field Types
                    context.FieldTypes.AddOrUpdate(
                       p => p.Name,
                       new FieldType() { Name = "Texto" },
                       new FieldType() { Name = "Imagem" },
                       new FieldType() { Name = "Checkbox" },
                       new FieldType() { Name = "Html" },
                       new FieldType() { Name = "Galeria" },
                       new FieldType() { Name = "Repetidor" },
                       new FieldType() { Name = "Seleção" },
                       new FieldType() { Name = "Midia" }
                    );
                    #endregion

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}