namespace SmartPantry.Despensa;

public static class DespensaConsts
{
    // Regla de aviso de vencimiento (RF-15): se avisa cuando faltan DiasUmbralAvisoVencimiento
    // días o menos (umbral inclusivo). Las fechas se comparan en UTC por día calendario.
    public const int DiasUmbralAvisoVencimiento = 3;
}
