//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré à partir d'un modèle.
//
//     Des modifications manuelles apportées à ce fichier peuvent conduire à un comportement inattendu de votre application.
//     Les modifications manuelles apportées à ce fichier sont remplacées si le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AsposePdfEditor
{
    using System;
    using System.Collections.Generic;
    
    public partial class LIGNES_COMMANDES
    {
        public int id_ligne_commande { get; set; }
        public int id_commande { get; set; }
        public string designation { get; set; }
        public string reference { get; set; }
        public Nullable<int> quantite { get; set; }
        public string num_devis { get; set; }
        public Nullable<int> id_fournisseur { get; set; }
        public string num_facture_client { get; set; }
        public string num_facture_fournisseur { get; set; }
        public Nullable<System.DateTime> date_livraison { get; set; }
        public Nullable<System.DateTime> date_reception { get; set; }
        public Nullable<System.DateTime> date_facture_client { get; set; }
        public Nullable<System.DateTime> date_rglt_client { get; set; }
        public Nullable<System.DateTime> date_rglt_fournisseur { get; set; }
    }
}
