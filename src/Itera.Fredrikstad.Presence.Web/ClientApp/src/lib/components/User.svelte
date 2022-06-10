<script lang="ts">
  import { useQuery } from "@sveltestack/svelte-query";
  import { getUser } from "../api";

  $: query = useQuery(["user"], () => getUser());
  $: name = $query?.data?.name ?? "";
  $: photo = $query?.data?.photo ?? "";
  name = name;
  photo = photo;
</script>

<div class="name-container">
  <p>{name}</p>
  <img class="avatar" src={photo} alt={`Profile photo of ${name}`} />
</div>

<style>
  .name-container {
    display: flex;
    flex-direction: row;
    gap: 12px;
  }

  .avatar {
    width: 48px;
    height: 48px;
    border-radius: 24px;
  }
</style>
