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
    
    public partial class BWALL_FILES
    {
        public int IDFILE { get; set; }
        public Nullable<bool> AFFAIRE { get; set; }
        public string HASHMASTERINIT { get; set; }
        public string HASHDEPOSANT { get; set; }
        public string HASHSOCIETE { get; set; }
        public string HASHINITIATEUR { get; set; }
        public string COMMENTAIRE { get; set; }
        public string HASHFILE { get; set; }
        public Nullable<System.DateTime> DATE_CREATION { get; set; }
        public string HEURE_CREATION { get; set; }
        public Nullable<int> IDRUBRIQUE { get; set; }
        public Nullable<int> IDSOUSRUBRIQUE { get; set; }
        public string HASHENCHARGE { get; set; }
        public Nullable<System.DateTime> DATE_PRISE_EN_CHARGE { get; set; }
        public Nullable<bool> CLOTURE { get; set; }
        public Nullable<System.DateTime> DATE_CLOTURE { get; set; }
        public string HEURE_CLOTURE { get; set; }
        public string HASHCLOTURANT { get; set; }
        public Nullable<bool> INTERNE { get; set; }
        public Nullable<int> IDCATEGORIE { get; set; }
        public Nullable<System.Guid> CATEGORIE { get; set; }
        public string COMMCLOTURE { get; set; }
        public string COMMCLOTURECOMMERCIAL { get; set; }
        public string TICKET { get; set; }
        public string HASHTECHPRECEDENT { get; set; }
        public Nullable<int> ETAPE { get; set; }
        public string TYPEINTER { get; set; }
        public Nullable<bool> ENTITY { get; set; }
        public Nullable<int> PRIORITY { get; set; }
        public string OLDCOMMENTAIRE { get; set; }
        public Nullable<int> OLDCATEGORIE { get; set; }
        public string MASTERKEY { get; set; }
        public Nullable<bool> LOCKED { get; set; }
        public Nullable<bool> DIRIGE { get; set; }
        public Nullable<System.Guid> IDTACHE { get; set; }
        public string DUREE { get; set; }
        public string TYPECONTRAT { get; set; }
    }
}