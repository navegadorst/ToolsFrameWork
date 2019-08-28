using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices;
using System.Collections;
using System.DirectoryServices.AccountManagement;

namespace ToolsFrameWork.Tools
{
    /// <summary>
    /// clase para validar usuario en active directory
    /// esta clase imlementa LDAP
    /// valida un grupo para la aplicación
    /// Valida el perfil del usuario 
    /// </summary>
    public class ToolActiveDirectory
    {
        private ResponseAutenticated responseAutenticated;
        public ToolActiveDirectory(){ }
        public ResponseAutenticated isAuthenticated(string domain, string usr, string pwd, string groupapp)
        {
            responseAutenticated = new ResponseAutenticated();
            bool authenticated = false;
            //is use ldap
            string strldap = string.Format("LDAP://{0}", domain);

            string pathGroupOK = string.Empty;
            string pathGroupProfilOK = string.Empty;
            try
            {
                DirectoryEntry entry = new DirectoryEntry(strldap, usr, pwd);
                object nativeobject = entry.NativeObject;
                authenticated = true;

                if (authenticated)
                {
                    pathGroupOK = GetGroup(groupapp, entry);
                    if (!string.IsNullOrEmpty(pathGroupOK))
                    {
                        pathGroupProfilOK = GetGroupProfile(groupapp, usr, entry);
                        if (!string.IsNullOrEmpty(pathGroupProfilOK))
                            responseAutenticated.isAutenticate = isActiveAccount(domain, usr);
                    }
                }

            }
            catch (Exception ex)
            {
                responseAutenticated.msjExecption = ex.Message;
                return responseAutenticated;
            }
            return responseAutenticated;
        }

        /// <summary>
        /// se busca en especifico el grupo de la aplicación
        /// </summary>
        /// <param name="groupname"></param>
        /// <param name="docentry"></param>
        /// <returns></returns>
        internal string GetGroup(string groupname, DirectoryEntry docentry)
        {
            DirectoryEntry rootEntry = docentry;
            string filterPath = string.Empty;
            try
            {

                DirectorySearcher srch = new DirectorySearcher(rootEntry);
                srch.SearchScope = SearchScope.Subtree;

                srch.Filter = "(&(objectCategory=Group)(name=" + groupname + "))";

                SearchResult resFilter = srch.FindOne();
                if (resFilter != null)
                    filterPath = resFilter.Path;
            }
            catch (Exception ex)
            {
                filterPath = string.Empty;
                responseAutenticated.msjExecption = ex.Message;

            }

            return filterPath;
        }

        /// <summary>
        /// busca el perfil del grupo de la aplicación y usuario
        /// </summary>
        /// <param name="groupname"></param>
        /// <param name="usr"></param>
        /// <param name="docentry"></param>
        /// <returns></returns>
        internal string GetGroupProfile(string groupname, string usr, DirectoryEntry docentry)
        {
            DirectoryEntry rootEntry = docentry;
            string strprofile = string.Empty;

            try
            {
                string queryusr = string.Format("(sAMAccountName={0})", usr);

                DirectorySearcher srch = new DirectorySearcher(rootEntry, queryusr);
                srch.SearchScope = SearchScope.Subtree;
                SearchResult res = srch.FindOne();

                if (res != null)
                {
                    DirectoryEntry obUser = new DirectoryEntry(res.Path);
                    object obGroups = obUser.Invoke("Groups");
                    foreach (object ob in (IEnumerable)obGroups)
                    {
                        DirectoryEntry obGpEntry = new DirectoryEntry(ob);
                        strprofile = ValidProfile(obGpEntry.Name, groupname);
                        if (!string.IsNullOrEmpty(strprofile))
                            break;

                    }
                }
            }
            catch (Exception ex)
            {
                strprofile = string.Empty;
                responseAutenticated.msjExecption = ex.Message;
            }
            responseAutenticated.strprofile = strprofile;
            return strprofile;
        }
        /// <summary>
        /// Valida el perfil , este proceso es especifico para este desrrollo 
        /// </summary>
        /// <param name="groupprofilename"></param>
        /// <param name="groupapplication"></param>
        /// <returns></returns>
        internal string ValidProfile(string groupprofilename, string groupapplication)
        {
            string profile = string.Empty;
            var remplaceProfile = groupprofilename.Replace("CN=", "");
            var compareGroupApp = remplaceProfile.Contains(groupapplication) ?
                                    remplaceProfile.Replace(groupapplication, "") :
                                    string.Empty;
            if (!string.IsNullOrEmpty(compareGroupApp))
            {
                compareGroupApp = compareGroupApp.Substring(1);
                compareGroupApp = compareGroupApp.Replace("_", " ");
            }

            profile = compareGroupApp;
            return profile;
        }

        /// <summary>
        /// optiene el estatus de usuario 
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="usr"></param>
        /// <returns></returns>
        internal bool isActiveAccount(string domain, string usr)
        {
            var domainContext = new PrincipalContext(ContextType.Domain, domain);
            var foundUser = UserPrincipal.FindByIdentity(domainContext, IdentityType.SamAccountName, usr);
            bool isActiveUser = false;
            if (foundUser != null)
                if (foundUser.Enabled.HasValue)
                    isActiveUser = (bool)foundUser.Enabled;

            responseAutenticated.isStatuActive = isActiveUser;
            return isActiveUser;
        }
    }
    public class ResponseAutenticated
    {
        /// <summary>
        /// mensaje de exception
        /// </summary>
        public string msjExecption { get; set; } = string.Empty;
        /// <summary>
        /// confirmacion de autenticación exitosa
        /// </summary>
        public bool isAutenticate { get; set; } = false;
        /// <summary>
        /// validador de estatus de usuario
        /// </summary>
        public bool isStatuActive { get; set; } = false;
        /// <summary>
        /// información de perfil si existe
        /// </summary>
        public string strprofile { get; set; } = string.Empty;

    }
}
