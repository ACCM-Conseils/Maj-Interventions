using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AsposePdfEditor
{
    public partial class SUPPORTS_INTERVENTIONS
    {
        public SUPPORTS_INTERVENTIONS()
        {
            ticket = string.Empty;
            date_intervention = DateTime.Today;
            duree_intervention = string.Empty;
            descriptif = string.Empty;
            actions = string.Empty;
            commercial = string.Empty;
            contrat = true;
            contratastreinte = false;
            horscontrat = false;
            depannage = false;
            entretien = false;
            config = false;
            sauvegarde = false;
            serveur = false;
            posteclient = false;
            securite = false;
            reseau = false;
            scan = false;
            messagerie = false;
            moa = false;
            moe = false;
        }
    }
}