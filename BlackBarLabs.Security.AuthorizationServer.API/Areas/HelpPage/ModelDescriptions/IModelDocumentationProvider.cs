using System;
using System.Reflection;

namespace BlackBarLabs.Security.AuthorizationServer.API.Areas.HelpPage.ModelDescriptions
{
    public interface IModelDocumentationProvider
    {
        string GetDocumentation(MemberInfo member);

        string GetDocumentation(Type type);
    }
}